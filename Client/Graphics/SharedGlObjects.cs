using System.Text.Json;
using System.Text.Json.Serialization;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers.UI;
using Bergmann.Client.InputHandlers;
using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Bergmann.Client.Graphics;


public static class SharedGlObjects {
    public static Program UIProgram { get; private set; } = null!;
    public static Program BlockProgram { get; private set; } = null!;
    public static void CompilePrograms() {
        DeletePrograms();

        Shader Vertex = new(ShaderType.VertexShader);
        Shader Fragment = new(ShaderType.FragmentShader);
        Vertex.Compile(ResourceManager.ReadFile(ResourceManager.Type.Shaders, "Block.vert"));
        Fragment.Compile(ResourceManager.ReadFile(ResourceManager.Type.Shaders, "Block.frag"));

        BlockProgram = new();
        BlockProgram.AddShader(Vertex);
        BlockProgram.AddShader(Fragment);
        BlockProgram.Compile();
        BlockProgram.OnLoad += () => {
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            Matrix4 projMat = Matrix4.CreatePerspectiveFieldOfView(1.0f, (float)Window.Instance.Size.X / Window.Instance.Size.Y, 0.1f, 300f);
            projMat.M11 = -projMat.M11; //this line inverts the x display direction so that it uses our x: LHS >>>>> RHS
            BlockProgram.SetUniform("projection", projMat);
            BlockProgram.SetUniform("model", Matrix4.Identity);

            GL.ActiveTexture(TextureUnit.Texture0);
            BlockTextures.Bind();
            GL.Uniform1(GL.GetUniformLocation(BlockProgram.Handle, "stack"), 0);
            GlLogger.WriteGLError();
        };

        Vertex.Dispose();
        Fragment.Dispose();

        Vertex = new(ShaderType.VertexShader);
        Fragment = new(ShaderType.FragmentShader);
        Vertex.Compile(ResourceManager.ReadFile(ResourceManager.Type.Shaders, "UI.vert"));
        Fragment.Compile(ResourceManager.ReadFile(ResourceManager.Type.Shaders, "UI.frag"));

        UIProgram = new();
        UIProgram.AddShader(Vertex);
        UIProgram.AddShader(Fragment);
        UIProgram.Compile();

        UIProgram.OnLoad += () => {
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest); //this is required so that ui elements may overlap
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            UIProgram.SetUniform("windowsize", Window.Instance.Size);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.ActiveTexture(TextureUnit.Texture1);
            UIProgram.SetUniform("textStack", 0);
            UIProgram.SetUniform("textureUni", 1);
            GlLogger.WriteGLError();
        };

        Vertex.Dispose();
        Fragment.Dispose();
    }
    public static void DeletePrograms() {
        UIProgram?.Dispose();
        BlockProgram?.Dispose();
    }


    /// <summary>
    /// The texture stack is a 2d array of textures, compiled out of a supplied json file.
    /// It can be bound and every block can find its appropriate textures if the layer specified in <see cref="BlockInfo"/> is used as the texture coordiante's
    /// z component.
    /// </summary>
    public static TextureStack BlockTextures { get; private set; } = null!;
    public static void AssembleBlockTextures(string filename) {
        DropBlockTextures();
        BlockTextures = new();

        using JsonDocument doc = JsonDocument.Parse(
            ResourceManager.ReadFile(ResourceManager.Type.Jsons, filename));

        JsonSerializerOptions options = new() {
            Converters = {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };


        JsonElement arrayNode = doc.RootElement.GetProperty("Textures");


        int size = doc.RootElement.GetProperty("Size").GetInt32();
        BlockTextures.Reserve(size, size, arrayNode.GetArrayLength());

        foreach (JsonElement x in arrayNode.EnumerateArray()) {
            try {
                using Image texture = Image.Load(ResourceManager.FullPath(ResourceManager.Type.Textures, x.GetProperty("Texture").GetString()!));
                using Image<Rgba32> tex = texture.CloneAs<Rgba32>();
                BlockTextures.Write(tex, x.GetProperty("Layer").GetInt32());
            } catch (Exception e) {
                Logger.Warn($"Couldn't load texture from json file {filename} with {x}. \nException: {e}");
            }
        }
        GlLogger.WriteGLError();
    }
    public static void DropBlockTextures() {
        BlockTextures?.Dispose();
    }


    public static TextureStack LetterTextures { get; private set; } = null!;
    /// <summary>
    /// All the chars that can be rendered by the text renderer. If the used char is not known, OpenGl defaults to "a".
    /// Then you can just add it.
    /// </summary>
    public const string RenderableChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789, @(^%&$){}[]+=-?_*/.#:;\\<>|äöüÄÖÜß!'\"";
    
    /// <summary>
    /// Builds a new letter stack which can be used for fast text rendering in shaders.
    /// The layers identifier is given by their index in <see cref="RenderableChars"/>
    /// </summary>
    /// <param name="font">The font which is used to write the letters into the textures</param>
    /// <param name="size">The size of each layer (quadratic) in pixels. If the letter is not quadratic, the image
    /// is resized without keeping the same aspect ratio, 
    /// so that the texture can be unstretched and it gives the correct image</param>
    public static void AssembleLetterTextures() {
        DropLetterTextures();
        FontCollection collection = new();
        collection.Add(ResourceManager.FullPath(ResourceManager.Type.Fonts, "Consolas.ttf"));
        Font font = collection.Get("Consolas").CreateFont(70);
        LetterTextures = new();
        int size = 100;
        LetterTextures.Reserve(size, size, RenderableChars.Length);


        TextOptions options = new(font) {

        };

        for (int i = 0; i < RenderableChars.Length; i++) {
            string sub = RenderableChars[i].ToString();

            FontRectangle bounds = TextMeasurer.Measure(sub, options);
            using Image<Rgba32> img = new(Configuration.Default, (int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height));
            img.Mutate(x => x.BackgroundColor(Color.Transparent)
                .DrawText(options, sub, Brushes.Solid(Color.White), Pens.Solid(Color.Black, 2f))
                .Resize(size, size));

            LetterTextures.Write(img, i);
        }
        GlLogger.WriteGLError();
    }
    public static void DropLetterTextures() {
        LetterTextures?.Dispose();
    }


    public static Texture2D CrossTexture { get; set; } = null!;

    public static void MakeUITextures() {
        using Image<Rgba32> img = Image<Rgba32>.Load(
            ResourceManager.FullPath(ResourceManager.Type.Textures, "cross.png")).CloneAs<Rgba32>();
        CrossTexture = new();
        CrossTexture.Write(img);


        BoxRenderer CrossRenderer = new() {
            AbsoluteAnchorOffset = (0, 0),
            PercentageAnchorOffset = (0.5f, 0.5f),
            RelativeAnchor = (0.5f, 0.5f),
            Dimension = (100, 100)
        };
        CrossRenderer.ApplyLayout();
    }
    public static void DropUITextures() {
        CrossTexture?.Dispose();
    }


    public static void FreeAll() {
        DeletePrograms();
        DropBlockTextures();
        DropLetterTextures();
        DropUITextures();
    }
}