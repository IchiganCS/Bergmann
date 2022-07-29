using Bergmann.Client.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// Renders a small rolling diagram without axes on top of a box.
/// See <see cref="BoxRenderer"/> for additional details of positioning this box.
/// </summary>
public class DiagramRenderer : BoxRenderer {
    public DiagramRenderer(int tickInterval = 500, int ticksToShow = 50) {
        TickInterval = tickInterval;
        TicksToShow = ticksToShow;
        //TickTimer = new(x => Tick(), null, TickInterval, TickInterval);
        tex = new();
    }

    /// <summary>
    /// A list of floats always in the range of 0 to 1 to be displayed in the diagram.
    /// Values above and below this range are clamped.
    /// </summary>
    public IList<float> DataPoints { get; private set; } = new List<float>();

    /// <summary>
    /// When to forward the diagram automatically in milliseconds.
    /// </summary>
    public int TickInterval { get; private set; }
    /// <summary>
    /// How many ticks shall be displayed?
    /// </summary>
    public int TicksToShow { get; set; } = 100;
    private readonly Timer TickTimer;

    private readonly Texture2D tex;

    public void TickAndAddDataPoint(float value) {
        DataPoints.Add(value);
        Tick();
    }

    /// <summary>
    /// Sets all <see cref="DataPoints"/>.
    /// </summary>
    /// <param name="dataPoints">The new data points.</param>
    public void SetDataPoints(IList<float> dataPoints) {
        DataPoints = dataPoints;
    }
    /// <summary>
    /// Adds a new data point.
    /// </summary>
    /// <param name="value">The float (always between 0 and 1).</param>
    public void AddDataPoint(float value) {
        DataPoints.Add(value);
    }


    /// <summary>
    /// Advances the 
    /// </summary>
    private void Tick() {
        float widthOfTick = Dimension.X / TicksToShow;
        float remainingSpace = Dimension.X;

        Image<Rgba32> image = new((int)Dimension.X, (int)Dimension.Y, new Rgba32(0, 0, 0));

        //the diagram starts at the right and is rolled over with every tick
        foreach (float value in DataPoints) {
            if (remainingSpace < 0)
                break;

            float valueToDraw = value < 0 ? 0 : (value > 1 ? 1 : value); //Better is Math.Clamp
            image.Mutate(x => x.FillPolygon(Color.White,
                new PointF(0, Dimension.Y - remainingSpace),
                new PointF(valueToDraw * Dimension.X, Dimension.Y - remainingSpace),
                new PointF(0, Dimension.Y - remainingSpace - widthOfTick),
                new PointF(valueToDraw * Dimension.X, Dimension.Y - remainingSpace - widthOfTick)
                ));

            // A working solution seems to be:
            // Only OpenGL handles (0, 0) as bottom left, for most of image processing, (0, 0) refers to top(!) left.

            // image.Mutate(x => x.Fill(Color.White,
            //     new RectangleF(
            //         new PointF(Dimension.X - remainingSpace, (1 - valueToDraw) * Dimension.Y),
            //         new SizeF(widthOfTick, valueToDraw * Dimension.Y))
            // ));

            // it also seems nice to have a little bit of space between each bar.
            // stated above: The diagram starts at the right. Currently, it starts left.
            // ALTERNATIVELY: this fps display on the bottom right seems very interesting: https://i.ytimg.com/vi/Zza-iOlSlfQ/maxresdefault.jpg

            remainingSpace -= widthOfTick;
        }

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