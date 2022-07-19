using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers;
using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;


/// <summary>
/// Renders a texture on a box. This is a ui class, the box is two dimensional. 
/// It can display any two dimensional texture array and can as such be used to render text
/// indirectly. See <see cref="TextRenderer"/> for more info about that.
/// </summary>
public class BoxRenderer : IDisposable, IUIRenderer {

    /// <summary>
    /// The vertices for the box. It is filled with objects of <see cref="UIVertex"/>. 
    /// Take a look at it to see the options you have for the layout
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

    /// <summary>
    /// Absolute offset for the anchor
    /// </summary>
    public Vector2 AbsoluteAnchorOffset { get; set; }
    /// <summary>
    /// Percentage offset for the anchor
    /// </summary>
    public Vector2 PercentageAnchorOffset { get; set; }
    /// <summary>
    /// Defines an anchor for the box. (0,0) means the box's anchor is at the lower left, (1,0) is anchoring the box on the right
    /// </summary>
    public Vector2 RelativeAnchor { get; set; }
    /// <summary>
    /// The width and height of the box
    /// </summary>
    public Vector2 Dimension { get; set; }



    #pragma warning disable CS8618
    /// <summary>
    /// Constructs an empty box renderer
    /// </summary>
    /// <param name="estimateSections">How many sections this box renderer probably holds. It's not a hard boundary, 
    /// but nice to have for optimization</param>
    public BoxRenderer(int estimateSections = 1) {
        EnsureBufferCapacity(estimateSections);
    }
    #pragma warning restore CS8618



    /// <summary>
    /// Constructs <see cref="Vertices"/> and <see cref="Indices"/> for this box.
    /// This method can only work if a layout is applied.
    /// </summary>    
    /// <param name="separators">Each separator is a cut and symbolizes the beginning of the next texture in the stack. 
    /// The first float is the offset from the end of the last texture.
    /// The second float is the width of the new block.
    /// Overlapping is possible.
    /// The integer is the layer of the texture stack. 
    /// The cutting is done along the horizontal axis, so the cuts are vertical.</param>
    public void ApplyTexture(IEnumerable<(float, float, int)> separators) {

        Vector2 anchorOffset = new(-RelativeAnchor.X * Dimension.X, -RelativeAnchor.Y * Dimension.Y);

        EnsureBufferCapacity(separators.Count());

        List<UIVertex> vertices = new();
        List<uint> indices = new();

        float passedSpace = 0f;
        uint indexToUse = 0;
        foreach ((float, float, int) pair in separators) {
            Vector2 coveredWidth = new(passedSpace * Dimension.X + pair.Item1 * Dimension.X, 0);
            float spaceThisPass = pair.Item2 * Dimension.X;


            vertices.AddRange(new UIVertex[4] {
                new() { 
                    Absolute = coveredWidth + anchorOffset + AbsoluteAnchorOffset, 
                    Percent = PercentageAnchorOffset, 
                    TexCoord = new(0, 0, pair.Item3)},
                new() { 
                    Absolute = coveredWidth + anchorOffset + AbsoluteAnchorOffset + new Vector2(spaceThisPass, 0), 
                    Percent = PercentageAnchorOffset, 
                    TexCoord = new(1, 0, pair.Item3)},
                new() { 
                    Absolute = coveredWidth + anchorOffset + AbsoluteAnchorOffset + new Vector2(0, Dimension.Y), 
                    Percent = PercentageAnchorOffset, 
                    TexCoord = new(0, 1, pair.Item3)},
                new() { 
                    Absolute = coveredWidth + anchorOffset + AbsoluteAnchorOffset + new Vector2(spaceThisPass, Dimension.Y),
                    Percent = PercentageAnchorOffset, 
                    TexCoord = new(1, 1, pair.Item3)}});

            indices.AddRange(new uint[6] {
                indexToUse + 0, indexToUse + 1, indexToUse + 3, 
                indexToUse + 0, indexToUse + 2, indexToUse + 3
            });
            indexToUse += 4;
            passedSpace += pair.Item2 + pair.Item1;
        }

        Vertices.Fill(vertices.ToArray());
        Indices.Fill(indices.ToArray());
    }

    /// <summary>
    /// Constructs <see cref="Vertices"/> and <see cref="Indices"/> for this box.
    /// This method can only work if a layout is applied.
    /// </summary>    
    /// <param name="layer">Applies the given texture to the entire box. 
    /// It is the index used in the ui fragment shader for the texture stack</param>
    public void ApplyTexture(int layer)
        => ApplyTexture(new (float, float, int)[1] { (0f, 1f, layer) });


    /// <inheritdoc/>
    public bool PointInShape(Vector2 point, Vector2 windowsize) {

        Vector2 anchorOffset = new(-RelativeAnchor.X * Dimension.X, -RelativeAnchor.Y * Dimension.Y);
        Vector2 pctOffset = new(windowsize.X * PercentageAnchorOffset.X, windowsize.Y * PercentageAnchorOffset.Y);

        Vector2 startPoint = anchorOffset + AbsoluteAnchorOffset + pctOffset;

        Box2 box = new(startPoint, startPoint + Dimension);
        return box.Contains(point);
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