using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.Renderers.UI;


/// <summary>
/// A view to display some debug information. It just unites some text renderers.
/// </summary>
public class DebugRenderer : UIRenderer {
    /// <summary>
    /// The list of text renderers. They are vertically appended from the top left corner.
    /// </summary>
    private IList<TextRenderer> Texts { get; set; } = new List<TextRenderer>();

    /// <summary>
    /// Gives names to indices used in <see cref="Texts"/> for easy and unique access.
    /// </summary>
    private enum Identifiers {
        Position = 0,
        FPS = 1,
        Chunks = 2,
    }


    /// <summary>
    /// Generates a new debug renderer. It builds the layout for the required text fields.
    /// </summary>
    public DebugRenderer() {
        for (int i = 0; i < 3; i++) { //adjust count accordingly
            Texts.Add(new() {
                AbsoluteAnchorOffset = (30, -30 - i * 80),
                RelativeAnchor = (0, 1),
                PercentageAnchorOffset = (0, 1),
                Dimension = (-1, 70)
            });
            Texts[i].SetText("");
        }
    }


    /// <summary>
    /// Updates the values rendered. All of these values are required for a full debug information.
    /// </summary>
    /// <param name="position">The position of the player.</param>
    /// <param name="fps">The current fps.</param>
    public void Update(Vector3 position, float fps, int chunksLoaded) {
        Texts[(int)Identifiers.Position].SetText($"Pos:    ({position.X:0.00}, {position.Y:0.00}, {position.Z:0.00})");
        Texts[(int)Identifiers.FPS].SetText($"Fps:    {fps:0.00}");
        Texts[(int)Identifiers.Chunks].SetText($"Chunks: {chunksLoaded} loaded");
    }

    /// <summary>
    /// Renders all text renderers.
    /// </summary>
    public override void Render() {
        foreach (IRenderer ren in Texts)
            ren.Render();
    }


    /// <summary>
    /// Disposes all renderers used for rendering the debug view.
    /// </summary>
    public override void Dispose() {
        foreach (IRenderer renderer in Texts)
            renderer.Dispose();
    }
}