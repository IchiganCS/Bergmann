using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// A renderer with a special functions useful for ui.
/// </summary>
public interface IUIRenderer : IRenderer {

    /// <summary>
    /// Calculates wheter a given point lies in the rendered geometry.
    /// </summary>
    /// <param name="point">The point in the geometry. For UI, the z component is ignored</param>
    /// <param name="size">This value is the currently active window size and only required for UI rendering.</param>
    public bool PointInShape(Vector2 point, Vector2 size);
}