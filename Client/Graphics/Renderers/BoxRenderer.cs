using Bergmann.Client.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Shared;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Buffer = Bergmann.Client.Graphics.OpenGL.Buffer;


/// <summary>
/// Renders a texture on a box. This is a ui class, the box is two dimensional. It can work as a text renderer or any other texture.
/// </summary>
public class BoxRenderer : IDisposable {
    /// <summary>
    /// The texture to be strechted on the box. Could be a text on a transparent background
    /// </summary>
    private Texture Texture { get; set; }

    /// <summary>
    /// The vertices for the box. It is filled with object of <see cref="UIVertex"/>. Take a look at it to see the options you have for the layout
    /// of boxes. Should always be only 4 elements.
    /// </summary>
    private Buffer Vertices { get; set; }

    /// <summary>
    /// The indices. Could be made static?
    /// </summary>
    private Buffer Indices { get; set; }

    private static FontCollection FontCollection { get; set; } = new();
    private static Font DebugFont { get; set; }

    /// <summary>
    /// Constructs <see cref="Vertices"/> and <see cref="Indices"/> for this box.
    /// </summary>
    /// <param name="originAbs">Absolute offset</param>
    /// <param name="originPct">Percentage offset</param>
    /// <param name="anchor">Defines an anchor for the box. (0,0) means the box's anchor is at the lower left, (1,0) is anchoring the box on the right</param>
    /// <param name="texture">The width and height of the texture. It's required to calculate the layout</param>
    private void MakeBoxFor(Vector2 originAbs, Vector2 originPct, Vector2 anchor, Vector2 texture) {
        Vector2 anchorOffset = new(-anchor.X * texture.X, -anchor.Y * texture.Y);

        Vertices.Fill(new UIVertex[4] {
            new() { Absolute = anchorOffset + originAbs, Percent = originPct, TexCoord = new(0, 0)},
            new() { Absolute = anchorOffset + originAbs + new Vector2(texture.X, 0), Percent = originPct, TexCoord = new(1, 0)},
            new() { Absolute = anchorOffset + originAbs + new Vector2(0, texture.Y), Percent = originPct, TexCoord = new(0, 1)},
            new() { Absolute = anchorOffset + originAbs + new Vector2(texture.X, texture.Y), Percent = originPct, TexCoord = new(1, 1)},
        });
        Indices.Fill(new uint[6] {
            0, 1, 3,
            0, 2, 3
        });
    }

    /// <summary>
    /// It generates behind the scenes object, therefore needs to run on the main thread.
    /// </summary>
    public BoxRenderer() {
        Vertices = new Buffer(BufferTarget.ArrayBuffer);
        Indices = new Buffer(BufferTarget.ElementArrayBuffer);
        Texture = new Texture(TextureTarget.Texture2D);
    }


    /// <summary>
    /// The box renderer for now is also the source for the text renderer. It could be further distinguished in the future if the text renderer component becomes
    /// too large.
    /// </summary>
    static BoxRenderer() {
        FontCollection.Add(ResourceManager.FullPath(ResourceManager.Type.Fonts, "Consolas.ttf"));

        DebugFont = FontCollection.Get("Consolas").CreateFont(75); //the pixel size is 4/5 * this given font size
    }


    #pragma warning disable CS8618
    /// <summary>
    /// Makes a text renderer.
    /// </summary>
    /// <param name="originAbs">The absolute offset from the lower left corner</param>
    /// <param name="originPct">The percentage offset from the lower left corner</param>
    /// <param name="anchor">Defines an anchor for the text. (0,0) means the text's anchor is at the lower left, (1,0) is anchoring the text on the right</param>
    public static BoxRenderer MakeTextRenderer(string text, Vector2 originAbs, Vector2 originPct, Vector2 anchor) {
        BoxRenderer boxRenderer = new();

        TextOptions options = new(DebugFont) {

        };

        FontRectangle bounds = TextMeasurer.Measure(text, options);
        using Image<Rgba32> img = new(Configuration.Default, (int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height));
        img.Mutate(x => x.BackgroundColor(Color.Transparent));
        img.Mutate(x => x.DrawText(options, text, Brushes.Solid(Color.White), Pens.Solid(Color.Black, 1f)));

        boxRenderer.Texture.Write(img);

        boxRenderer.MakeBoxFor(originAbs, originPct, anchor, new(bounds.Width, bounds.Height));

        return boxRenderer;
    }
    #pragma warning disable CS8618


    /// <summary>
    /// Renders the box and its texture. Make sure the UI program is bound.
    /// </summary>
    /// <param name="tu">The texture unit used. It defaults to zero, if you change it, make sure the UI program uses it</param>
    public void Render(TextureUnit tu = TextureUnit.Texture0) {
        GL.ActiveTexture(TextureUnit.Texture0);
        Texture.Bind();
        Vertices.Bind();
        UIVertex.UseVAO();
        Indices.Bind();

        GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
    }


    public void Dispose() {
        Texture.Dispose();
        Vertices.Dispose();
        Indices.Dispose();
    }
}