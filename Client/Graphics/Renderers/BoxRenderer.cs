using Bergmann.Client.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Buffer = Bergmann.Client.Graphics.OpenGL.Buffer;


/// <summary>
/// Renders a texture on a box. This is a ui class, the box is two dimensional. It can display any two dimensional texture array and can as such be used to render text
/// indirectly. See <see cref="TextRenderer"/> for more info about that.
/// </summary>
public class BoxRenderer : IDisposable {

    /// <summary>
    /// The vertices for the box. It is filled with objects of <see cref="UIVertex"/>. Take a look at it to see the options you have for the layout
    /// of boxes. Since each box can be separated into different textures, this buffer can hold 4 * num_of_cuts.
    /// </summary>
    private Buffer Vertices { get; set; }

    /// <summary>
    /// The indices for the vertices.
    /// </summary>
    private Buffer Indices { get; set; }


    /// <summary>
    /// Constructs <see cref="Vertices"/> and <see cref="Indices"/> for this box.
    /// </summary>
    /// <param name="originAbs">Absolute offset for the anchor</param>
    /// <param name="originPct">Percentage offset for the anchor</param>
    /// <param name="anchor">Defines an anchor for the box. (0,0) means the box's anchor is at the lower left, (1,0) is anchoring the box on the right</param>
    /// <param name="dimension">The width and height of the box. It's required to calculate the layout</param>
    /// <param name="separators">If the parameter is supplied (is not null), then the texture is assumed to be an array. The values have to be between 0 and 1. Each separator is a cut
    /// and symbolizes the beginning of the next texture. The integer is the layer the texture is connected. The cutting is done along the horizontal axis, so the cuts are vertical.
    /// All the first items of the pairs should be one when summed up</param>
    /// <param name="layer">Layer is used if not separators are supplied. It's the layer in the bound texture stack</param>
    public BoxRenderer(Vector2 originAbs, Vector2 originPct, Vector2 anchor, Vector2 dimension, IEnumerable<(float, int)>? separators = null, int layer = -1) {
        Vertices = new Buffer(BufferTarget.ArrayBuffer);
        Indices = new Buffer(BufferTarget.ElementArrayBuffer);

        Vector2 anchorOffset = new(-anchor.X * dimension.X, -anchor.Y * dimension.Y);
        if (separators is null) {
            Vertices.Fill(new UIVertex[4] {
                new() { Absolute = anchorOffset + originAbs, Percent = originPct, TexCoord = new(0, 0, layer)},
                new() { Absolute = anchorOffset + originAbs + new Vector2(dimension.X, 0), Percent = originPct, TexCoord = new(1, 0, layer)},
                new() { Absolute = anchorOffset + originAbs + new Vector2(0, dimension.Y), Percent = originPct, TexCoord = new(0, 1, layer)},
                new() { Absolute = anchorOffset + originAbs + new Vector2(dimension.X, dimension.Y), Percent = originPct, TexCoord = new(1, 1, layer)},
            });
            Indices.Fill(new uint[6] {
                0, 1, 3,
                0, 2, 3
            });
        }
        else {
            List<UIVertex> vertices = new();
            List<int> indices = new();

            float passedSpace = 0f;
            int indexToUse = 0;
            foreach ((float, int) pair in separators) {
                Vector2 coveredWidth = new(passedSpace * dimension.X, 0);
                float spaceThisPass = pair.Item1 * dimension.X;
                vertices.AddRange(new UIVertex[4] {
                    new() { Absolute = coveredWidth + anchorOffset + originAbs, Percent = originPct, TexCoord = new(0, 0, pair.Item2)},
                    new() { Absolute = coveredWidth + anchorOffset + originAbs + new Vector2(spaceThisPass, 0), Percent = originPct, TexCoord = new(1, 0, pair.Item2)},
                    new() { Absolute = coveredWidth + anchorOffset + originAbs + new Vector2(0, dimension.Y), Percent = originPct, TexCoord = new(0, 1, pair.Item2)},
                    new() { Absolute = coveredWidth + anchorOffset + originAbs + new Vector2(spaceThisPass, dimension.Y), Percent = originPct, TexCoord = new(1, 1, pair.Item2)}});
                indices.AddRange(new int[6] {
                    indexToUse + 0, indexToUse + 1, indexToUse + 3, 
                    indexToUse + 0, indexToUse + 2, indexToUse + 3
                });
                indexToUse += 4;
                passedSpace += pair.Item1;
            }

            Vertices.Fill(vertices.ToArray());
            Indices.Fill(indices.ToArray());
        }
    }


    /// <summary>
    /// Renders the box and its texture. Make sure the UI program is bound. Make sure an appropriate texture is bound
    /// </summary>
    public virtual void Render() {
        Vertices.Bind();
        UIVertex.UseVAO();
        Indices.Bind();
        GlLogger.WriteGLError();

        GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
        GlLogger.WriteGLError();
    }


    public void Dispose() {
        Vertices.Dispose();
        Indices.Dispose();
    }
}