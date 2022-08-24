using Bergmann.Client.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;


namespace Bergmann.Client.Graphics.Renderers.UI;

/// <summary>
/// Renders a texture on a box. This is a ui class, the box is two dimensional.
/// The box is not connected to the box and should be bound to the non stack texture before rendering.
/// </summary>
public class BoxRenderer : UIRenderer {

    private VertexArray<UIVertex>? VAO { get; set; }


    /// <summary>
    /// Constructs an empty box renderer and allocates the buffers.
    /// </summary>
    public BoxRenderer() {
        GlThread.Invoke(() => {
            VAO = new(
                new Buffer<UIVertex>(BufferTarget.ArrayBuffer, 4),
                new Buffer<uint>(BufferTarget.ElementArrayBuffer, 6));
        });
    }



    /// <summary>
    /// Constructs vertices and indices for this box.
    /// This method can only work if a layout is set (e.g. <see cref="UIRenderer.Dimension"/> etc. is set).
    /// </summary>
    public void ApplyLayout(int textureLayer = 0) {
        Vector2 bottomLeftOffset = -RelativeAnchor * Dimension + AbsoluteAnchorOffset;

        GlThread.Invoke(() => {
            VAO?.IndexBuffer.Fill(new uint[6] {
                0, 1, 3,
                0, 2, 3
            });

            VAO?.VertexBuffer.Fill(new UIVertex[4] {
                new() {
                    Absolute = bottomLeftOffset,
                    Percent = PercentageAnchorOffset,
                    TexCoord = new(0, 0, textureLayer)},
                new() {
                    Absolute = bottomLeftOffset + new Vector2(Dimension.X, 0),
                    Percent = PercentageAnchorOffset,
                    TexCoord = new(1, 0, textureLayer)},
                new() {
                    Absolute = bottomLeftOffset + new Vector2(0, Dimension.Y),
                    Percent = PercentageAnchorOffset,
                    TexCoord = new(0, 1, textureLayer)},
                new() {
                    Absolute = bottomLeftOffset + new Vector2(Dimension.X, Dimension.Y),
                    Percent = PercentageAnchorOffset,
                    TexCoord = new(1, 1, textureLayer)}
            });
        });
    }


    /// <summary>
    /// Renders the box. Make sure an appropriate texture is bound to the used texture slot.
    /// </summary>
    /// <param name="useStack">Whether to use the texture stack or a non-stack texture.</param>
    public void Render(bool useStack) {
        Program.Active?.SetUniform("useStack", useStack);

        VAO?.Draw();
    }

    public override void Render()
        => Render(useStack: false);


    public override void Dispose()
        => VAO?.Dispose();
}