using OpenTK.Mathematics;

namespace Bergmann.Shared.Noise;

/// <summary>
/// A classic perlin2d noise sampler as described by Wikipedia (https://en.wikipedia.org/wiki/Perlin_noise).
/// </summary>
public class Perlin2D : INoise<Vector2> {

    /// <summary>
    /// A function returning a random vector for any given position. This method shall return the same vector for the same position.
    /// </summary>
    public Func<Vector2, Vector2> VectorRandomizer { get; private set; }

    /// <summary>
    /// The rate at which a vector is sampled. E.g. if set to 16, a vector is only calculated every sixteenth step.
    /// This is quadratic, the sample rate applies to width and height. The point at (0, 0) is always sampled and builds
    /// the origin.
    /// </summary>
    public float SampleRate { get; private set; }

    /// <summary>
    /// The maximal distance of a position to the next sample vector.
    /// </summary>
    private float MaxSampleDistance { get; set; }

    /// <summary>
    /// Constructs a new perlin noise sampler.
    /// </summary>
    /// <param name="vectorRandomizer">The function to randomize a vector at a given position.</param>
    /// <param name="sampleRate">The space whenever a new vector shall be sampled.</param>
    public Perlin2D(Func<Vector2, Vector2> vectorRandomizer, float sampleRate) {
        VectorRandomizer = vectorRandomizer;
        SampleRate = sampleRate;

        MaxSampleDistance = (float)Math.Sqrt(2 * SampleRate * SampleRate);
    }


    /// <summary>
    /// A mathematical function to form a smooth step from [0, 1], so that the harshness of linearity in the noise is reduced.
    /// </summary>
    /// <param name="val">The value to be smooth stepped.</param>
    /// <returns>A value in [0, 1].</returns>
    private static float SmootherStep(float val) {
        //https://en.wikipedia.org/wiki/Smoothstep#Variations
        if (val < 0)
            return 0;
        if (val > 1)
            return 1;

        return val * val * val * (val * (val * 6 - 15) + 10);
    }

    private static float Interpolate(float val1, float val2, float weight) {
        return val1 + SmootherStep(weight) * (val2 - val1);
    }

    public float Sample(Vector2 position) {

        //the special perlin vectors to interpolate the value from
        Vector2[] perlins = new Vector2[4];
        //the offsets where those vectors where sampled
        Vector2[] offsets = new Vector2[4];

        //now calculate the neighbors: first, find the exact positions where to calculate them.
        Vector2 sampleCount = position / SampleRate;

        //this sorting is important. The first two values are the bottom values, the last two, the top values.
        offsets[0] = new Vector2((float)Math.Floor(sampleCount.X), (float)Math.Floor(sampleCount.Y)) * SampleRate;
        offsets[1] = new Vector2((float)Math.Floor(sampleCount.X + 1), (float)Math.Floor(sampleCount.Y)) * SampleRate;
        offsets[2] = new Vector2((float)Math.Floor(sampleCount.X ), (float)Math.Floor(sampleCount.Y + 1)) * SampleRate;
        offsets[3] = new Vector2((float)Math.Floor(sampleCount.X + 1), (float)Math.Floor(sampleCount.Y + 1)) * SampleRate;

        //now calculate the vectors - this could be improved by caching them in the future.
        for (int i = 0; i < 4; i++)
            perlins[i] = VectorRandomizer(offsets[i]);


        //how each vector should be weighted
        Vector2 weights = (position - offsets[0]) / SampleRate;

        //find the contribution of each vector on the position.
        //directly interpolate on the way first horizontally. Make use of the sorting of offsets.
        float bottomVal = Interpolate(
            Vector2.Dot((position - offsets[0]) / MaxSampleDistance, perlins[0]),
            Vector2.Dot((position - offsets[1]) / MaxSampleDistance, perlins[1]), 
            weights.X);

        float topVal = Interpolate(
            Vector2.Dot((position - offsets[2]) / MaxSampleDistance, perlins[2]),
            Vector2.Dot((position - offsets[3]) / MaxSampleDistance, perlins[3]), 
            weights.X);

        return Interpolate(bottomVal, topVal, weights.Y);
    }
}