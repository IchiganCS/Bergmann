using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.InputHandlers;
using OpenTK.Graphics.OpenGL;
using Shared;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// Renders a text on top of a box. It makes use of <see cref="BoxRenderer.ApplyTexture"/> method, so there's no need to call it.
/// Additionally, when specifying the layout of the box, you only have to specify the y coordinate of 
/// <see cref="BoxRenderer.Dimension"/>. In case of the text renderer being hooked up to a <see cref="TextHandler"/>, 
/// it renders the cursor automatically.
/// </summary>
public class TextRenderer : BoxRenderer {
#pragma warning disable CS8618
    private static FontCollection FontCollection { get; set; } = new();
    private static Font DebugFont { get; set; }
    private static Texture DebugFontStack { get; set; }
#pragma warning restore CS8618

    /// <summary>
    /// All the chars that can be rendered by the text renderer. If the used char is not known, OpenGl defaults to "a".
    /// Then you can just add it.
    /// </summary>
    private const string CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789, @(^%&$){}[]+=-?_*/.#:;\\<>|äöüÄÖÜß!'\"";

    /// <summary>
    /// Returns a new letter stack which can be used for fast rendering in shaders.
    /// The texture's type is a 2d array. The layers identifier is given by their index in <see cref="CHARS"/>
    /// </summary>
    /// <param name="font">The font which is used to write the letters into the textures</param>
    /// <param name="size">The size of each layer (quadratic) in pixels. If the letter is not quadratic, the image
    /// is resized without keeping the same aspect ratio, 
    /// so that the texture can be unstretched and it gives the correct image</param>
    /// <returns>A Texture2DArray. The caller needs to dispose of it</returns>
    private static Texture MakeLetterStack(Font font, int size = 50) {
        Texture stack = new(TextureTarget.Texture2DArray);
        stack.Reserve(size, size, CHARS.Length);


        TextOptions options = new(font) {

        };

        for (int i = 0; i < CHARS.Length; i++) {
            string sub = CHARS[i].ToString();

            FontRectangle bounds = TextMeasurer.Measure(sub, options);
            using Image<Rgba32> img = new(Configuration.Default, (int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height));
            img.Mutate(x => x.BackgroundColor(Color.Transparent)
                .DrawText(options, sub, Brushes.Solid(Color.White), Pens.Solid(Color.Black, 2f))
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

        DebugFontStack = MakeLetterStack(DebugFont, 100);
    }

    /// <summary>
    /// Renders a given text into the box renderer. The x component of <see cref="BoxRenderer.Dimension"/> is discarded to 
    /// fit the text
    /// </summary>
    /// <param name="text">The text to be rendered</param>
    /// <param name="cursor">The position of the cursor to be rendered. If less than zero, then ignored</param>
    public void SetText(string text, int cursor = -1) {
        if (cursor >= 0) {
            text = text.Insert(cursor, "|");
        }

        float widthOfOne = Dimension.Y * 0.7f;
        float entireWidth = widthOfOne * text.Length;
        Dimension = new(entireWidth, Dimension.Y);
        ApplyTexture(text.Select((c, i) => {
            //make overlapping sections on the box renderer for the cursor
            float width = 1f / text.Length;
            float cursorWidth = width;
            float cursorOffset = -0.5f * width;

            int charLayer = CHARS.IndexOf(c);

            if (i != cursor && i != cursor + 1)
                return (0, width, charLayer);

            else if (i == cursor)
                return (cursorOffset, cursorWidth, charLayer);

            else
                return (-cursorWidth - cursorOffset, width, charLayer);
        }));
    }

    /// <summary>
    /// Hooks the text field to this text renderer. No further action on the text renderer is then required.
    /// </summary>
    /// <param name="tf">The text field whose values are checked on every update</param>
    public void HookTextField(TextHandler tf) {
        SetText(tf.Text, tf.Cursor);
        tf.OnUpdate += 
            () => SetText(tf.Text, tf.Cursor);
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