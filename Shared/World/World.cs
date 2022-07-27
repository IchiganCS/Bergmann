using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace Bergmann.Shared.World;

public class World {

    /// <summary>
    /// Uses the Key for each Chunk. Look up <see cref="Chunk.Key"/>. Stores each chunk currently loaded.
    /// </summary>
    public Dictionary<long, Chunk> Chunks { get; set; }


    public Generator WorldGen { get; private set; }

    public World() {
        Chunks = new();
        WorldGen = new((int)Random.Shared.NextInt64());
    }

    public void LoadChunk(long key) {
        if (Chunks.ContainsKey(key))
            return;

        Vector3i offset = Chunk.ComputeOffset(key);
        if (offset.Y < 0)
            return;

        Chunk newChunk = new() {
            Key = key,
            Blocks = WorldGen.GenerateBlocksForChunk(key),
        };
        Chunks.Add(key, newChunk);
    }

    /// <summary>
    /// Ensures that all chunks in the required distance are loaded
    /// </summary>
    /// <param name="position">The position from which to calculate</param>
    /// <param name="distance">The distance in world chunk space, the number of chunks</param>
    public static IEnumerable<long> GetNearChunks(Vector3 position, int distance) {

        // this stores all offsets
        List<Vector3i> chunksInRange = new() {
            (Vector3i)position
        };

        for (int i = 0; i < chunksInRange.Count; i++) {
            //pop the highest element
            Vector3i current = chunksInRange[i];

            foreach (Block.Face face in Block.AllFaces) {
                Vector3i offset = 16 * Block.FaceToVector[(int)face];

                //the offset of the chunk in world space
                Vector3i world = offset + current;

                if (world.Y < 0 || ((Vector3i)(world - position)).ManhattanLength > distance * 16
                    || chunksInRange.Contains(world))
                    continue;

                chunksInRange.Add(world);
            }
        }

        return chunksInRange.Select(x => Chunk.ComputeKey(x));
    }


    /// <summary>
    /// Gets the chunk for the given <see cref="Chunk.Key"/>.
    /// </summary>
    /// <param name="key">The key as given by <see cref="Chunk.ComputeKey"/></param>
    /// <returns>The chunk, null if the chunk is not loaded</returns>
    public Chunk? GetChunk(long key) {
        if (Chunks.ContainsKey(key))
            return Chunks[key];
        return null;
    }

    public Block GetBlockAt(Vector3i position) {
        long key = Chunk.ComputeKey(position);
        if (!Chunks.ContainsKey(key))
            return 0;

        Chunk chunk = Chunks[key];
        return chunk.GetBlockWorld(position);
    }

    public void SetBlockAt(Vector3i position, Block block) {
        long key = Chunk.ComputeKey(position);
        if (!Chunks.ContainsKey(key))
            return;

        Chunk chunk = Chunks[key];
        chunk.SetBlockWorld(position, block);
    }

    /// <summary>
    /// Cast a ray from origin in the direction of destination. Returns whether there has been a hit and if that value 
    /// is true, the hit face and position of that block is returned. Since this logically needs to be distance
    /// limited, the limit is <paramref name="distance"/>.
    /// </summary>
    /// <param name="origin">The origin of the ray. If origin lies in a block, that same block is returned</param>
    /// <param name="direction">The direction shot from origin</param>
    /// <param name="distance">The distance when to end the raycast.</param>
    /// <param name="hitBlock">The position of the block hit by the raycast.</param>
    /// <param name="hitFace">The hit face of the block.</param>
    /// <returns>Whether there was a hit in <paramref name="distance"/>.</returns>
    public bool Raycast(Vector3 origin, Vector3 direction, out Vector3i hitBlock, out Block.Face hitFace, float distance = 5) {
        //this method works like this:
        //We use the Block.GetFaceFromHit method to walk through each face that lies along direction.
        //We truly walk along every block - quite elegant.

        Vector3 position = origin;

        //but because we could get stuck on exactly an edge and possibly hit the same cube over and over again
        //we have to add a slight delta to move into the direction after each block move
        Vector3 directionDelta = direction;
        directionDelta.NormalizeFast();
        directionDelta /= 100f;

        int i = (int)distance * 10;
        while ((position - origin).LengthSquared < distance * distance) {
            i--;

            Vector3i flooredPosition = new(
                (int)Math.Floor(position.X),
                (int)Math.Floor(position.Y),
                (int)Math.Floor(position.Z));

            Block current = GetBlockAt(flooredPosition);
            if (current != 0) {
                hitBlock = flooredPosition;
                hitFace = Block.GetFaceFromHit(position - flooredPosition, direction, out _);
                return true;
            }

            _ = Block.GetFaceFromHit(position - flooredPosition, -direction, out Vector3 hit);

            if (i <= 0) {
                Logger.Warn("Something went wrong, returning no hit");
                break;
            }

            position = hit + flooredPosition + directionDelta;
        }

        hitFace = Block.Face.Front;
        hitBlock = (0, 0, 0);
        return false;
    }


    public class Generator {
        public int Seed { get; private set; }
        public Random Random { get; private set; }

        public SortedDictionary<int, Vector2> PerlinVectors { get; private set; }

        public const int LOW_BOUND = 0;
        public const int TERRAIN_LEVEL = 35;

        public Generator(int seed) {
            Random = new Random(seed);
            Seed = seed;
            PerlinVectors = new();
        }



        public int[,,] GenerateBlocksForChunk(long key) {
            int[,,] result = new int[16, 16, 16];
            Vector3i offset = Chunk.ComputeOffset(key);

            Vector2[,] perlinVectors = new Vector2[2, 2];
            for (int x = 0; x < 2; x++) {
                for (int z = 0; z < 2; z++) {
                    int seed = ((x * 16 + offset.X) << 16) + (z * 16 + offset.Z);
                    Random rand = new(seed);
                    float angle = rand.NextSingle() * 2 * (float)Math.PI;
                    perlinVectors[x, z] = new Vector2((float)Math.Sin(angle), (float)Math.Cos(angle));
                }
            }

            for (int x = 0; x < 16; x++) {
                for (int z = 0; z < 16; z++) {
                    Vector2 closestPerlinVec = perlinVectors[x < 8 ? 0 : 1, z < 8 ? 0 : 1];
                    float value = Vector2.Dot((new Vector2(x, z) - new Vector2(x < 8 ? 0 : 16, z < 8 ? 0 : 16)), closestPerlinVec);
                    int maxHeight = TERRAIN_LEVEL + (int)(value * 8);
                    for (int y = 0; y < 16; y++) {
                        result[x, y, z] = maxHeight > offset.Y + y ? 1 : 0;
                    }
                }
            }


            return result;
        }
    }
}