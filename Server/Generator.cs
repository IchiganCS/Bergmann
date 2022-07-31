using Bergmann.Shared.Noise;
using Bergmann.Shared.World;
using OpenTK.Mathematics;

namespace Bergmann.Server;

public class Generator {
    public int Seed { get; private set; }
    private INoise<Vector2> ChunkNoise { get; set; }
    private INoise<Vector2> MountainNoise { get; set; }


    public const int LOW_BOUND = 0;
    public const int TERRAIN_LEVEL = 35;

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



    public Chunk? GenerateChunk(long key) {
        Chunk result = new();

        // https://en.wikipedia.org/wiki/Perlin_noise
        int[,,] blocks = new int[16, 16, 16];
        Vector3i offset = Chunk.ComputeOffset(key);
        
        if (offset.Y < 0)
            return null;



        for (int x = 0; x < 16; x++) {
            for (int z = 0; z < 16; z++) {
                float maxHeight = TERRAIN_LEVEL + ChunkNoise.Sample(new Vector2(x, z) + offset.Xz) * 4 + MountainNoise.Sample(new Vector2(x, z) + offset.Xz) * 50;

                for (int y = 0; y < 16; y++) {
                    blocks[x, y, z] = maxHeight > offset.Y + y ? 1 : 0;
                }
            }
        }

        result.Key = key;
        result.Blocks = blocks;
        return result;
    }
}