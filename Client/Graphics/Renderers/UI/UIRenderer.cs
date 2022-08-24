using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.Renderers.UI;

/// <summary>
/// A renderer with a special functions useful for ui.
/// </summary>
public abstract class UIRenderer : IRenderer {

    /// <summary>
    /// Absolute offset for the anchor
    /// </summary>
    public Vector2 AbsoluteAnchorOffset { get; set; }
    /// <summary>
    /// Percentage offset for the anchor
    /// </summary>
    public Vector2 PercentageAnchorOffset { get; set; }
    /// <summary>
    /// Defines an anchor for the box. (0,0) means the box's anchor is at the lower left, 
    /// (1,0) is anchoring the box on the right
    /// </summary>
    public Vector2 RelativeAnchor { get; set; }
    /// <summary>
    /// The width and height of the box in pixels.
    /// </summary>
    public Vector2 Dimension { get; set; }



    /// <summary>
    /// Calculates wheter a given point lies in the rendered geometry.
    /// </summary>
    /// <param name="point">The point in the geometry. For UI, the z component is ignored</param>
    /// <param name="size">This value is the currently active window size and only required for UI rendering.</param>
    public bool PointInShape(Vector2 point, Vector2 size) { 
        Vector2 anchorOffset = -RelativeAnchor * Dimension;
        Vector2 pctOffset = size * PercentageAnchorOffset;

        Vector2 startPoint = anchorOffset + AbsoluteAnchorOffset + pctOffset;

        Box2 box = new(startPoint, startPoint + Dimension);
        return box.Contains(point);
    }


    public abstract void Render();
    public abstract void Dispose();
}