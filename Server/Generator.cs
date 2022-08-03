using Bergmann.Shared.Noise;
using Bergmann.Shared.Objects;
using OpenTK.Mathematics;

namespace Bergmann.Server;


/// <summary>
/// A class that can generate any chunk in a world.
/// It can of course be inherited and made more complex.
/// </summary>
public class Generator {

    /// <summary>
    /// The seed of the world so that any world generation with the same seed yields the same result.
    /// </summary>
    /// <value></value>
    public int Seed { get; private set; }

    /// <summary>
    /// A noise to make a chunk a little more noisy.
    /// </summary>
    private INoise<Vector2> ChunkNoise { get; set; }

    /// <summary>
    /// A big noise: Very heavy and slow, a very noticable influence. It can model mountains.
    /// </summary>
    private INoise<Vector2> MountainNoise { get; set; }

    /// <summary>
    /// The lowest bound. If a block is below this value, it is not considered.
    /// </summary>
    public const int LOW_BOUND = 0;

    /// <summary>
    /// The average terrain level.
    /// </summary>
    public const int TERRAIN_LEVEL = 35;

    /// <summary>
    /// Makes a new generator with a given seed.
    /// </summary>
    /// <param name="seed">The seed of the world.</param>
    public Generator(int seed) {
        Seed = seed;

        ChunkNoise = new Perlin2D(pos => {
            int seed = (Seed + (int)pos.X << 16, Seed + (int)pos.Y << 16).GetHashCode();
            Random rand = new(seed);
            float angle = rand.NextSingle() * 2 * (float)Math.PI;
            return ((float)Math.Sin(angle), (float)Math.Cos(angle));
        }, 16f);
        MountainNoise = new Perlin2D(pos => {
            int seed = (Seed + (int)pos.X << 16, Seed + (int)pos.Y << 16).GetHashCode();
            Random rand = new(seed);
            float angle = rand.NextSingle() * 2 * (float)Math.PI + 0.1f;
            return ((float)Math.Sin(angle), (float)Math.Cos(angle));
        }, 16f * 10);
    }


    /// <summary>
    /// Generates a new chunk.
    /// </summary>
    /// <param name="key">The key of the chunk to be generated.</param>
    /// <returns>The finished chunk. null if there was any error or invalid request.</returns>
    public Chunk? GenerateChunk(long key) {
        Chunk result = new();

        // https://en.wikipedia.org/wiki/Perlin_noise
        int[,,] blocks = new int[16, 16, 16];
        Vector3i offset = Chunk.ComputeOffset(key);
        
        if (offset.Y < LOW_BOUND)
            return null;



        for (int x = 0; x < 16; x++) {
            for (int z = 0; z < 16; z++) {
                Vector2 samplePos = new Vector2(x, z) + offset.Xz;
                float maxHeight = TERRAIN_LEVEL 
                    + ChunkNoise.Sample(samplePos) * 4 
                    + MountainNoise.Sample(samplePos) * 50;

                for (int y = 0; y < 16; y++) {
                    int currentY = offset.Y + y;
                    blocks[x, y, z] = maxHeight > currentY ? (currentY > TERRAIN_LEVEL ? 1 : 2) : 0;
                }
            }
        }

        result.Key = key;
        result.Blocks = blocks;
        return result;
    }
}