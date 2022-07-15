using Bergmann.Client.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Shared;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Bergmann.Client.Graphics.Renderers;

public class TextRenderer : BoxRenderer {
    #pragma warning disable CS8618
    private static FontCollection FontCollection { get; set; } = new();
    private static Font DebugFont { get; set; }
    private static Texture DebugFontStack { get; set; }
    #pragma warning restore CS8618
    private const string CHARS = " abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789@(){}+=-*/.#:\\<>";

    /// <summary>
    /// Returns a new letter stack which can be used for fast rendering in shaders.
    /// The texture's type is a 2d array. The layers identifier is given by their index in <see cref="CHARS"/>
    /// </summary>
    /// <param name="font">The font which is used to write the letters into the textures</param>
    /// <param name="size">The size of each layer (quadratic) in pixels. If the letter is not quadratic, the image
    /// is resized without keeping the same aspect ratio, so that the texture can be unstretched and it gives the correct image</param>
    /// <returns>A Texture2DArray. The caller needs to dispose of it</returns>
    public static Texture MakeLetterStack(Font font, int size = 50) {
        Texture stack = new Texture(TextureTarget.Texture2DArray);
        stack.Reserve(size, size, CHARS.Length);


        TextOptions options = new(font) {

        };

        for (int i = 0; i < CHARS.Length; i++) {
            string sub = CHARS[i].ToString();

            FontRectangle bounds = TextMeasurer.Measure(sub, options);
            using Image<Rgba32> img = new(Configuration.Default, (int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height));
            img.Mutate(x => x.BackgroundColor(Color.Transparent)
                .DrawText(options, sub, Brushes.Solid(Color.White), Pens.Solid(Color.Black, 1.4f))
                .Resize(size, size));
                
            stack.Write(img, i);
        }
        GlLogger.WriteGLError();

        return stack;
    }

    /// <summary>
    /// Load required fonts and creates their letter stacks. Call <see cref="Dispose"/> afterwards.
    /// </summary>
    public static void Initialize() {
        FontCollection.Add(ResourceManager.FullPath(ResourceManager.Type.Fonts, "Consolas.ttf"));
        DebugFont = FontCollection.Get("Consolas").CreateFont(70);

        DebugFontStack = MakeLetterStack(DebugFont, 50);
    }

    public float TextHeight { get; private set; }

    /// <summary>
    /// Constructs a new TextRenderer.
    /// </summary>
    /// <param name="text">The text to be rendered</param>
    /// <param name="height">The height of the text.</param>
    /// <param name="originAbs">Absolute offset for the anchor</param>
    /// <param name="originPct">Percentage offset for the anchor</param>
    /// <param name="anchor">Defines an anchor for the box. (0,0) means the box's anchor is at the lower left, (1,0) is anchoring the box on the right</param>
    public TextRenderer(string text, float height, Vector2 originAbs, Vector2 originPct, Vector2 anchor) :
        base(text.Length) {
        TextHeight = height;
        MakeLayout(originAbs, originPct, anchor, new Vector2(TextHeight * text.Length, TextHeight), separators: text.Select((c, i) => ((1f / text.Length), CHARS.IndexOf(c))));

    }

    public void SetText(string text) {
        MakeLayout(AbsoluteAnchorOffset, PercentageAnchorOffset, RelativeAnchor, new Vector2(TextHeight * text.Length, TextHeight), separators: text.Select((c, i) => ((1f / text.Length), CHARS.IndexOf(c))));
    }

    /// <summary>
    /// Renders the underlying box renderer with the specfic text on it. Binds the letter stack to texture unit
    /// </summary>
    public override void Render() {
        GL.ActiveTexture(TextureUnit.Texture0);
        DebugFontStack.Bind();
        GlLogger.WriteGLError();
        base.Render();
    }


    public static void Delete() {
        DebugFontStack.Dispose();
    }
}