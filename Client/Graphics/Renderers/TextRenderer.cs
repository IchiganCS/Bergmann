using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.InputHandlers;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
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
public class TextRenderer : UIRenderer {
#pragma warning disable CS8618
    private static FontCollection FontCollection { get; set; } = new();
    private static Font DebugFont { get; set; }
    private static TextureStack DebugFontStack { get; set; }
#pragma warning restore CS8618

    /// <summary>
    /// All the chars that can be rendered by the text renderer. If the used char is not known, OpenGl defaults to "a".
    /// Then you can just add it.
    /// </summary>
    public const string CHARS = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789, @(^%&$){}[]+=-?_*/.#:;\\<>|äöüÄÖÜß!'\"";

    /// <summary>
    /// Returns a new letter stack which can be used for fast rendering in shaders.
    /// The layers identifier is given by their index in <see cref="CHARS"/>
    /// </summary>
    /// <param name="font">The font which is used to write the letters into the textures</param>
    /// <param name="size">The size of each layer (quadratic) in pixels. If the letter is not quadratic, the image
    /// is resized without keeping the same aspect ratio, 
    /// so that the texture can be unstretched and it gives the correct image</param>
    /// <returns>A texture stack. The caller needs to dispose of it</returns>
    private static TextureStack MakeLetterStack(Font font, int size = 50) {
        TextureStack stack = new();
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
    /// The vertices for the box. It is filled with objects of <see cref="UIVertex"/>. 
    /// Take a look at it to see the options you have for the layout
    /// of boxes. Since each box can be separated into different textures, this buffer can hold 4 * num_of_cuts.
    /// </summary>
    private Buffer<UIVertex>? Vertices { get; set; }

    /// <summary>
    /// The indices for the vertices.
    /// </summary>
    private Buffer<uint>? Indices { get; set; }


    /// <summary>
    /// Ensures that buffers are large enought to hold sections many items.
    /// Regenerates if necessary.
    /// </summary>
    /// <param name="sections">The number of sections for the box renderer</param>
    private void EnsureBufferCapacity(int sections) {
        if (Vertices is null || Indices is null) {
            Vertices?.Dispose();
            Indices?.Dispose();

            Vertices = new Buffer<UIVertex>(BufferTarget.ArrayBuffer, sections * 4);
            Indices = new Buffer<uint>(BufferTarget.ElementArrayBuffer, sections * 6);
        }

        else if (Vertices.Reserved < sections * 4 || Indices.Reserved < sections * 6) {
            Vertices.Dispose();
            Indices.Dispose();

            Vertices = new Buffer<UIVertex>(BufferTarget.ArrayBuffer, sections * 4);
            Indices = new Buffer<uint>(BufferTarget.ElementArrayBuffer, sections * 6);
        }
    }

    /// <summary>
    /// Renders a given text into the box renderer. The x component of <see cref="BoxRenderer.Dimension"/> is discarded to 
    /// fit the text. Each letter is given a box around it and a layer int the stack. For each letter, we therefore achieve 4 vertices.
    /// Plus 4 if a cursor is supplied.
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


        Vector2 anchorOffset = -RelativeAnchor * Dimension;

        EnsureBufferCapacity(text.Length);

        List<UIVertex> vertices = new();
        List<uint> indices = new();

        //the index to use for the next box
        uint indexToUse = 0;

        for (int i = 0; i < text.Length; i++) {
            char ch = text[i];
            int layer = CHARS.IndexOf(ch);
            float coveredSpace = i * widthOfOne;
            float spaceThisPass = widthOfOne;
            float cursorOffset = 0f;

            if (i == cursor)
                cursorOffset = -0.5f * widthOfOne;
            if (i > cursor && cursor > 0)
                cursorOffset = -1f * widthOfOne;

            vertices.AddRange(new UIVertex[4] {
                new() {
                    Absolute = anchorOffset + AbsoluteAnchorOffset + new Vector2(coveredSpace + cursorOffset, 0),
                    Percent = PercentageAnchorOffset,
                    TexCoord = new(0, 0, layer)},
                new() {
                    Absolute = anchorOffset + AbsoluteAnchorOffset + new Vector2(spaceThisPass + coveredSpace + cursorOffset, 0),
                    Percent = PercentageAnchorOffset,
                    TexCoord = new(1, 0, layer)},
                new() {
                    Absolute = anchorOffset + AbsoluteAnchorOffset + new Vector2(coveredSpace + cursorOffset, Dimension.Y),
                    Percent = PercentageAnchorOffset,
                    TexCoord = new(0, 1, layer)},
                new() {
                    Absolute = anchorOffset + AbsoluteAnchorOffset + new Vector2(spaceThisPass + coveredSpace + cursorOffset, Dimension.Y),
                    Percent = PercentageAnchorOffset,
                    TexCoord = new(1, 1, layer)}});

            indices.AddRange(new uint[6] {
                indexToUse + 0, indexToUse + 1, indexToUse + 3,
                indexToUse + 0, indexToUse + 2, indexToUse + 3
            });
            indexToUse += 4;
        }

        Vertices?.Fill(vertices.ToArray());
        Indices?.Fill(indices.ToArray());
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
    /// Renders the underlying box renderer with the specfic text on it. Binds the letter stack to the texture stack slot.
    /// </summary>
    public override void Render() {
        DebugFontStack.Bind();
        Program.Active!.SetUniform("useStack", true);

        Vertices?.Bind();
        UIVertex.BindVAO();
        Indices?.Bind();
        GlLogger.WriteGLError();

        GL.DrawElements(PrimitiveType.Triangles, Indices!.Length, DrawElementsType.UnsignedInt, 0);
        GlLogger.WriteGLError();
    }


    public override void Dispose() {
        Vertices?.Dispose();
        Indices?.Dispose();
    }

    public static void Delete() {
        DebugFontStack.Dispose();
    }
}