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


public class TextRenderer : IDisposable {
    private Texture Texture { get; set; }
    private Buffer Vertices { get; set; }
    private Buffer Indices { get; set; }

    private static FontCollection FontCollection { get; set; } = new();
    private static Font DebugFont { get; set; }
    private void MakeDebugText(string text, Vector2 originAbs, Vector2 originPct, Vector2 pctOffset) {
        Texture = new(TextureTarget.Texture2D);

        TextOptions options = new(DebugFont) {

        };

        FontRectangle bounds = TextMeasurer.Measure(text, options);
        using Image<Rgba32> img = new(Configuration.Default, (int)Math.Ceiling(bounds.Width), (int)Math.Ceiling(bounds.Height));
        img.Mutate(x => x.BackgroundColor(Color.Transparent));
        img.Mutate(x => x.DrawText(options, text, Brushes.Solid(Color.White), Pens.Solid(Color.Black, 1f)));

        img.Save("test.png");

        Texture.Write(img);

        Vector2 anchorOffset = new(-pctOffset.X * bounds.Width, -pctOffset.Y * bounds.Height);

        UIVertex[] vertices = new UIVertex[4] {
            new() { Absolute = anchorOffset + originAbs, Percent = originPct, TexCoord = new(0, 0)},
            new() { Absolute = anchorOffset + originAbs + new Vector2(bounds.Width, 0), Percent = originPct, TexCoord = new(1, 0)},
            new() { Absolute = anchorOffset + originAbs + new Vector2(0, bounds.Height), Percent = originPct, TexCoord = new(0, 1)},
            new() { Absolute = anchorOffset + originAbs + new Vector2(bounds.Width, bounds.Height), Percent = originPct, TexCoord = new(1, 1)},
        };
        uint[] indices = new uint[6] {
            0, 1, 3,
            0, 2, 3
        };

        Vertices = new Buffer(BufferTarget.ArrayBuffer);
        Indices = new Buffer(BufferTarget.ElementArrayBuffer);

        Vertices.Fill(vertices);
        Indices.Fill(indices);
    }

    static TextRenderer() {
        FontCollection.Add(ResourceManager.FullPath(ResourceManager.Type.Fonts, "Consolas.ttf"));

        DebugFont = FontCollection.Get("Consolas").CreateFont(75); //the pixel size is 4/5 * this given font size
    }

    /// <summary>
    /// Makes a text renderer.
    /// </summary>
    /// <param name="text">The text to be rendered by the renderer</param>
    /// <param name="originPct">The percentage offset from the lower left corner</param>
    /// <param name="originAbs">The absolute offset from the lower left corner</param>
    /// <param name="pctOffset">Defines an anchor for the text. (0,0) means the text's anchor is at the lower left, (1,0) is anchoring the text on the right</param>
    public TextRenderer(string text, Vector2 originAbs, Vector2 originPct, Vector2 pctOffset) {
        MakeDebugText(text, originAbs, originPct, pctOffset);
    }

    public void Render() {
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