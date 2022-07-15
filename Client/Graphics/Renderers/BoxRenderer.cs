using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;


/// <summary>
/// Renders a texture on a box. This is a ui class, the box is two dimensional. It can display any two dimensional texture array and can as such be used to render text
/// indirectly. See <see cref="TextRenderer"/> for more info about that.
/// </summary>
public class BoxRenderer : IDisposable, IRenderer {

    /// <summary>
    /// The vertices for the box. It is filled with objects of <see cref="UIVertex"/>. Take a look at it to see the options you have for the layout
    /// of boxes. Since each box can be separated into different textures, this buffer can hold 4 * num_of_cuts.
    /// </summary>
    private Buffer<UIVertex> Vertices { get; set; }

    /// <summary>
    /// The indices for the vertices.
    /// </summary>
    private Buffer<uint> Indices { get; set; }


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


    public Vector2 AbsoluteAnchorOffset { get; private set; }
    public Vector2 PercentageAnchorOffset { get; private set; }
    public Vector2 RelativeAnchor { get; private set; }
    public Vector2 Dimension { get; private set; }

    #pragma warning disable CS8618
    /// <summary>
    /// Constructs an empty box renderer
    /// </summary>
    /// <param name="estimateSections">How many sections this box renderer probably holds. It's not a hard boundary, but nice to have for optimization</param>
    public BoxRenderer(int estimateSections = 1) {
        EnsureBufferCapacity(estimateSections);
    }
    #pragma warning restore CS8618

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
    public void MakeLayout(Vector2 originAbs, Vector2 originPct, Vector2 anchor, Vector2 dimension, IEnumerable<(float, int)>? separators = null, int layer = -1) {

        Dimension = dimension;
        AbsoluteAnchorOffset = originAbs;
        PercentageAnchorOffset = originPct;
        RelativeAnchor = anchor;

        Vector2 anchorOffset = new(-RelativeAnchor.X * Dimension.X, -RelativeAnchor.Y * Dimension.Y);

        if (separators is null) {
            EnsureBufferCapacity(1);
            Vertices.Fill(new UIVertex[4] {
                new() { Absolute = anchorOffset + AbsoluteAnchorOffset, Percent = PercentageAnchorOffset, TexCoord = new(0, 0, layer)},
                new() { Absolute = anchorOffset + AbsoluteAnchorOffset + new Vector2(Dimension.X, 0), Percent = PercentageAnchorOffset, TexCoord = new(1, 0, layer)},
                new() { Absolute = anchorOffset + AbsoluteAnchorOffset + new Vector2(0, Dimension.Y), Percent = PercentageAnchorOffset, TexCoord = new(0, 1, layer)},
                new() { Absolute = anchorOffset + AbsoluteAnchorOffset + new Vector2(Dimension.X, Dimension.Y), Percent = PercentageAnchorOffset, TexCoord = new(1, 1, layer)},
            });
            Indices.Fill(new uint[6] {
                0, 1, 3,
                0, 2, 3
            });
        }
        else {
            EnsureBufferCapacity(separators.Count());

            List<UIVertex> vertices = new();
            List<uint> indices = new();

            float passedSpace = 0f;
            uint indexToUse = 0;
            foreach ((float, int) pair in separators) {
                Vector2 coveredWidth = new(passedSpace * Dimension.X, 0);
                float spaceThisPass = pair.Item1 * Dimension.X;
                vertices.AddRange(new UIVertex[4] {
                    new() { Absolute = coveredWidth + anchorOffset + AbsoluteAnchorOffset, Percent = PercentageAnchorOffset, TexCoord = new(0, 0, pair.Item2)},
                    new() { Absolute = coveredWidth + anchorOffset + AbsoluteAnchorOffset + new Vector2(spaceThisPass, 0), Percent = PercentageAnchorOffset, TexCoord = new(1, 0, pair.Item2)},
                    new() { Absolute = coveredWidth + anchorOffset + AbsoluteAnchorOffset + new Vector2(0, Dimension.Y), Percent = PercentageAnchorOffset, TexCoord = new(0, 1, pair.Item2)},
                    new() { Absolute = coveredWidth + anchorOffset + AbsoluteAnchorOffset + new Vector2(spaceThisPass, Dimension.Y), Percent = PercentageAnchorOffset, TexCoord = new(1, 1, pair.Item2)}});
                indices.AddRange(new uint[6] {
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