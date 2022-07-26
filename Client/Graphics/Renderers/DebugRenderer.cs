using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.Renderers;

public class DebugRenderer : IUIRenderer {
    public IList<TextRenderer> Texts { get; set; } = new List<TextRenderer>();
    private Func<Vector3> PosGetter { get; }
    private Func<float> FpsGetter { get; }

    private enum Identifiers {
        Position = 0,
        FPS = 1,
    }


    public DebugRenderer(Func<Vector3> posGetter, Func<float> fpsGetter) {
        Texts.Add(new() {
            AbsoluteAnchorOffset = (30, -30),
            RelativeAnchor = (0, 1),
            PercentageAnchorOffset = (0, 1),
            Dimension = (-1, 70)
        });
        Texts.Add(new() {
            AbsoluteAnchorOffset = (30, -120),
            RelativeAnchor = (0, 1),
            PercentageAnchorOffset = (0, 1),
            Dimension = (-1, 70)
        });
        PosGetter = posGetter;
        FpsGetter = fpsGetter;
    }

    private void Update() {
        Vector3 position = PosGetter();
        Texts[(int)Identifiers.Position].SetText($"Pos: ({position.X:0.00}, {position.Y:0.00}, {position.Z:0.00})");
        Texts[(int)Identifiers.FPS].SetText($"Fps: {FpsGetter()}");
    }

    public bool PointInShape(Vector2 point, Vector2 size) {
        return false; //disable all interaction with debug renderer for now.
    }

    public void Render() {
        Update();
        foreach (IRenderer ren in Texts)
            ren.Render();
    }

    public void Dispose() {
        foreach (IRenderer ren in Texts)
            ren.Dispose();
    }
}