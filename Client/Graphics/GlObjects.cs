using System.Text.Json;
using System.Text.Json.Serialization;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers.UI;
using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Bergmann.Client.Graphics;


/// <summary>
/// A unified class to hold static values required for all kinds of different objects. That may contain
/// texture stacks or different OpenGL programs and also some helper functions.
/// </summary>
public static class GlObjects {
    private static IList<string> SupportedExtensions { get; set; } = null!;

    /// <summary>
    /// A required operation to read all supported extensions. Call this on loading.
    /// </summary>
    public static void ReadSupportedExtensions() {
        int extensionCount = GL.GetInteger(GetPName.NumExtensions);
        SupportedExtensions = new List<string>(extensionCount);

        for (int i = 0; i < extensionCount; i++)
            SupportedExtensions.Add(GL.GetString(StringNameIndexed.Extensions, i));
    }

    /// <summary>
    /// Whether any extension identfied by its string is supported.
    /// </summary>
    /// <param name="extensionName">The name of the extension. Don't forget the GL_ part in front of it.</param>
    /// <returns>A boolean whether this extension is supported by the currently running context.</returns>
    public static bool SupportsExtension(string extensionName) {
        return SupportedExtensions.Contains(extensionName);
    }

    /// <summary>
    /// A ui program only useful for rendering ui elements.
    /// </summary>
    public static Program UIProgram { get; private set; } = null!;

    /// <summary>
    /// A block program with some default transformation and texturing logic.
    /// </summary>
    public static Program BlockProgram { get; private set; } = null!;

    /// <summary>
    /// Compiles all necessary programs with hard coded file name values.
    /// </summary>
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

            BlockProgram.SetUniform("model", Matrix4.Identity);

            GL.ActiveTexture(TextureUnit.Texture0);
            BlockTextures.Bind();
            BlockProgram.SetUniform("stack", 0);
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
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
 
            UIProgram.SetUniform("windowsize", Window.Instance.Size);
            UIProgram.SetUniform("textStack", 0);
            UIProgram.SetUniform("textureUni", 1);
        };

        Vertex.Dispose();
        Fragment.Dispose();
    }

    /// <summary>
    /// Frees up all porgrams by making them invalid. This method should only be called for clean up purposes.
    /// </summary>
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

    /// <summary>
    /// Reads all block textures from the default resource manager file name.
    /// </summary>
    public static void AssembleBlockTextures() {
        DropBlockTextures();
        BlockTextures = new();

        using JsonDocument doc = JsonDocument.Parse(
            ResourceManager.ReadFile(ResourceManager.Type.Jsons, "Textures.json"));

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
                Logger.Warn($"Couldn't load texture from json file \"Textures.json\" with {x}. \nException: {e}");
            }
        }
        GlLogger.WriteGLError();
    }
    /// <summary>
    /// Drops the texture for the blocks from memory.
    /// </summary>
    public static void DropBlockTextures() {
        BlockTextures?.Dispose();
    }


    /// <summary>
    /// A stack of textures which contain one letter per level. The index of the contained letters may be inferred from
    /// <see cref="RenderableChars"/>.
    /// </summary>
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
    /// <summary>
    /// Drops the letter stack from memory.
    /// </summary>
    public static void DropLetterTextures() {
        LetterTextures?.Dispose();
    }

    /// <summary>
    /// The cross in the middle. It might be replaced.
    /// </summary>
    public static Texture2D CrossTexture { get; set; } = null!;

    /// <summary>
    /// Loads all ui textures from memory using hard coded values.
    /// </summary>
    public static void MakeUITextures() {
        using Image<Rgba32> img = Image<Rgba32>.Load(
            ResourceManager.FullPath(ResourceManager.Type.Textures, "cross.png")).CloneAs<Rgba32>();
        CrossTexture = new();
        CrossTexture.Write(img);
    }

    /// <summary>
    /// Drops all ui textures from memory.
    /// </summary>
    public static void DropUITextures() {
        CrossTexture?.Dispose();
    }

    /// <summary>
    /// A helper function to build all required OpenGL objects.
    /// </summary>
    public static void BuildAll() {
        ReadSupportedExtensions();
        CompilePrograms();
        AssembleBlockTextures();
        AssembleLetterTextures();
        MakeUITextures();
    }

    /// <summary>
    /// A helper function to do all cleanup methods in one place. This might only be called when the client is closed.
    /// </summary>
    public static void FreeAll() {
        DeletePrograms();
        DropBlockTextures();
        DropLetterTextures();
        DropUITextures();
    }
}