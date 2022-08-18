using Bergmann.Client.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;


namespace Bergmann.Client.Graphics.Renderers.UI;

/// <summary>
/// Renders a texture on a box. This is a ui class, the box is two dimensional.
/// The box is not connected to the box and should be bound to the non stack texture before rendering.
/// </summary>
public class BoxRenderer : UIRenderer {

    private VertexArray<UIVertex> VAO { get; set; }


    /// <summary>
    /// Constructs an empty box renderer and allocates the buffers.
    /// </summary>
    public BoxRenderer() {
        VAO = new(
            new Buffer<UIVertex>(BufferTarget.ArrayBuffer, 4),
            new Buffer<uint>(BufferTarget.ElementArrayBuffer, 6));
    }



    /// <summary>
    /// Constructs <see cref="Vertices"/> and <see cref="Indices"/> for this box.
    /// This method can only work if a layout is set (e.g. <see cref="UIRenderer.Dimension"/> etc. is set) and
    /// should be called from the gl thread.
    /// </summary>
    public void ApplyLayout() {
        Vector2 anchorOffset = new(-RelativeAnchor.X * Dimension.X, -RelativeAnchor.Y * Dimension.Y);

        VAO.IndexBuffer.Fill(new uint[6] {
            0, 1, 3,
            0, 2, 3
        });
        VAO.VertexBuffer.Fill(new UIVertex[4] {
            new() {
                Absolute = anchorOffset + AbsoluteAnchorOffset,
                Percent = PercentageAnchorOffset,
                TexCoord = new(0, 0, 0)},
            new() {
                Absolute = anchorOffset + AbsoluteAnchorOffset + new Vector2(Dimension.X, 0),
                Percent = PercentageAnchorOffset,
                TexCoord = new(1, 0, 0)},
            new() {
                Absolute = anchorOffset + AbsoluteAnchorOffset + new Vector2(0, Dimension.Y),
                Percent = PercentageAnchorOffset,
                TexCoord = new(0, 1, 0)},
            new() {
                Absolute = anchorOffset + AbsoluteAnchorOffset + new Vector2(Dimension.X, Dimension.Y),
                Percent = PercentageAnchorOffset,
                TexCoord = new(1, 1, 0)}
        });
    }


    /// <summary>
    /// Renders the box. Make sure the UI program is bound. Make sure an appropriate texture is bound to the
    /// non-stack slot.
    /// </summary>
    public override void Render() {
        Program.Active?.SetUniform("useStack", false);
        GlLogger.WriteGLError();

        VAO.Draw();
        GlLogger.WriteGLError();
    }


    public override void Dispose()
        => VAO.Dispose();
}