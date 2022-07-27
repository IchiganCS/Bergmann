using Bergmann.Client.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// Renders a small rolling diagram without axes on top of a box.
/// It makes use of <see cref="BoxRenderer.ApplyTexture"/> method, so there's no need to call it.
/// See <see cref="TextRenderer"/> for additional details of positioning this box.
/// </summary>
public class DiagramRenderer : BoxRenderer {
    public DiagramRenderer(int tickInterval = 500, int ticksToShow = 50) {
        TickInterval = tickInterval;
        TicksToShow = ticksToShow;
        //TickTimer = new(x => Tick(), null, TickInterval, TickInterval);
        tex = new(TextureTarget.Texture2DArray);
        tex.Reserve(500, 70, 1);//TODO
        ApplyTexture(0);
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

    private readonly Texture tex;

    public void TickAndAddDataPoint(float value) {
        DataPoints.Add(value);
        Tick();
    }

    /// <summary>
    /// Sets all <see cref="DataPoints"/>.
    /// </summary>
    /// <param name="dataPoints">the new data points</param>
    public void SetDataPoints(IList<float> dataPoints) {
        DataPoints = dataPoints;
    }
    /// <summary>
    /// Adds a new data point
    /// </summary>
    /// <param name="value">the float (always between 0 and 1)</param>
    public void AddDataPoint(float value) {
        DataPoints.Add(value);
    }


    /// <summary>
    /// Advances the 
    /// </summary>
    private void Tick() {
        float widthOfTick = Dimension.Y / TicksToShow;
        float remainingSpace = Dimension.Y;

        Image<Rgba32> image = new((int)Dimension.X, (int)Dimension.Y, new Rgba32(0, 0, 0));

        //the diagram starts at the right and is rolled over with every tick
        foreach (float value in DataPoints) {
            remainingSpace -= widthOfTick;
            if (remainingSpace < 0)
                break;

            float valueToDraw = value < 0 ? 0 : (value > 1 ? 1 : value);
            image.Mutate(x => x.FillPolygon(Color.White,
                new PointF(0, Dimension.Y - remainingSpace),
                new PointF(valueToDraw * Dimension.X, Dimension.Y - remainingSpace),
                new PointF(0, Dimension.Y - remainingSpace - widthOfTick),
                new PointF(valueToDraw * Dimension.X, Dimension.Y - remainingSpace - widthOfTick)
                ));
        }

        tex.Write(image, 0);
    }


    /// <summary>
    /// Renders the underlying box renderer with the specfic text on it. Binds the letter stack to texture unit
    /// </summary>
    public override void Render() {
        GL.ActiveTexture(TextureUnit.Texture0);
        tex.Bind();
        GlLogger.WriteGLError();
        base.Render();
    }
}