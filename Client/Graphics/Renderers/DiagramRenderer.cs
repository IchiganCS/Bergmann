using Bergmann.Client.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// Renders a small rolling diagram without axes on top of a box.
/// See <see cref="BoxRenderer"/> for additional details of positioning this box.
/// It renders points in [0, 1] by vertical bars.
/// </summary>
public class DiagramRenderer : BoxRenderer {

    /// <summary>
    /// A new diagram renderer.
    /// </summary>
    /// <param name="ticksToShow">How many ticks should be display at most.</param>
    public DiagramRenderer(int ticksToShow = 300) {
        TicksToShow = ticksToShow;
        tex = new();
    }

    /// <summary>
    /// A list of floats always in the range of 0 to 1 to be displayed in the diagram.
    /// Values above and below this range are clamped.
    /// </summary>
    public IList<float> DataPoints { get; private set; } = new List<float>();

    /// <summary>
    /// How many ticks shall be displayed?
    /// </summary>
    public int TicksToShow { get; set; }

    /// <summary>
    /// The texture which stores the diagram.
    /// </summary>
    private readonly Texture2D tex;

    /// <summary>
    /// Adds a new data point and instantly remakes the image.
    /// </summary>
    /// <param name="value">The value to be added.</param>
    public void TickAndAddDataPoint(float value) {
        AddDataPoint(value);
        Tick();
    }

    /// <summary>
    /// Adds a new data point.
    /// </summary>
    /// <param name="value">The float (always between 0 and 1).</param>
    public void AddDataPoint(float value) {
        DataPoints.Add(Math.Clamp(value, 0, 1));
    }


    /// <summary>
    /// Renders all boxes to the texture. This method only renders given data points and also
    /// cuts a few data points if they exceed the necessary amount.
    /// </summary>
    private void Tick() {
        float widthOfTick = Dimension.X / TicksToShow;
        float remainingSpace = Dimension.X;

        Image<Rgba32> image = new((int)Dimension.X, (int)Dimension.Y, new Rgba32(0, 0, 0));

        //the diagram starts at the right and is rolled over with every tick
        foreach (float value in DataPoints.ToArray().Reverse()) {
            if (remainingSpace < 0)
                break;

            image.Mutate(x => x.Fill(Color.White,
                new RectangleF(
                    new PointF(remainingSpace - widthOfTick, (1 - value) * Dimension.Y),
                    new SizeF(widthOfTick, value * Dimension.Y))
            ));

            remainingSpace -= widthOfTick;
        }
        while (DataPoints.Count > 2 * TicksToShow)
            DataPoints = DataPoints.TakeLast(TicksToShow).ToList();

        tex.Write(image);
    }


    /// <summary>
    /// Binds the freshly made texture and renders the underlying box.
    /// </summary>
    public override void Render() {
        tex.Bind();
        base.Render();
    }
}