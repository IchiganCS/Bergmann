using OpenTK.Mathematics;

namespace Bergmann.Shared.World;

public class World {
    public static int Distance { get; private set; } = 5;
    public static int ColumnHeight { get; private set; } = 2;

    /// <summary>
    /// Holds Distance * Distance elements. To get a chunk at offset (n, ..., m), access n * Distance + m.
    /// The elements of the list are a vertical list (bottom to top) of a chunk column.
    /// </summary>
    public Dictionary<int, Chunk> Chunks { get; set; }

    public World() {
        Chunks = new();
    }

    public void InitChunks() {

        for (int i = 0; i < Distance * Distance; i++) {
            int x = i / Distance, z = i % Distance;

            for (int y = 0; y < ColumnHeight; y++) {
                Chunk newChunk = new() { Offset = new(x * 16, y * 16, z * 16) };
                if (Chunks.ContainsKey(newChunk.Key))
                    Chunks[newChunk.Key] = newChunk;
                else
                    Chunks.Add(newChunk.Key, newChunk);

                OnChunkLoading?.Invoke(newChunk);
            }
        }
    }


    public Block GetBlockAt(Vector3i position) {
        int key = Chunk.CalculateKey(position);
        if (!Chunks.ContainsKey(key))
            return 0;

        Chunk chunk = Chunks[key];
        return chunk.GetBlockWorld(position);
    }

    public void SetBlockAt(Vector3i position, Block block) {
        int key = Chunk.CalculateKey(position);
        if (!Chunks.ContainsKey(key))
            return;

        Chunk chunk = Chunks[key];
        chunk.SetBlockWorld(position, block);
    }

    /// <summary>
    /// Cast a ray from origin in the direction of destination. Returns whether there has been a hit through an out param
    /// and if that value is true, the hit face and position of that block is returned. Since this logically needs to be distance
    /// limited, the limit is currently that the position of the hit can only be 5 away from origin.
    /// </summary>
    /// <param name="origin">The origin of the ray. If origin lies in a block, that same block is returned</param>
    /// <param name="direction">The direction shot from origin</param>
    /// <param name="hasHit">Sets a boolean whether there has been a hit.</param>
    /// <returns></returns>
    public (Vector3i, Block.Face) Raycast(Vector3 origin, Vector3 direction, out bool hasHit) {
        //this method works like this:
        //We use the Block.GetFaceFromHit method to walk through each face that lies along direction.
        //We truly walk along every block - quite elegant.

        Vector3 position = origin;

        //but because we could get stuck on exactly an edge and possibly hit the same cube over and over again
        //we have to add a slight delta to move into the direction after each block move
        Vector3 directionDelta = direction;
        directionDelta.NormalizeFast();
        directionDelta /= 100f;

        int i = -1;
        while((position - origin).LengthSquared < 25) {
            i++;

            Vector3i flooredPosition = new(
                (int)Math.Floor(position.X),
                (int)Math.Floor(position.Y),
                (int)Math.Floor(position.Z));
            
            Block current = GetBlockAt(flooredPosition);
            if (current != 0) {
                hasHit = true;
                return (flooredPosition, Block.GetFaceFromHit(position - flooredPosition, direction, out _));
            }

            _ = Block.GetFaceFromHit(position - flooredPosition, -direction, out Vector3 hit);

            if (i > 50 ||
                (origin - hit - flooredPosition).LengthSquared <= (origin - position).LengthSquared) {
                //this means, this iteration didn't get nearer to the origin
                Logger.Warn("Something went wrong, returning no hit");
                hasHit = false;
                return (new(0, 0, 0), Block.Face.Front);
            }
            
            position = hit + flooredPosition + directionDelta;
        }
        
        hasHit = false;
        return (new(0, 0, 0), Block.Face.Front);
    }


    /// <summary>
    /// The delegate for the corresponding event
    /// </summary>
    /// <param name="newChunk">Chunk is fully initialized and already inserted</param>
    public delegate void ChunkLoadingDelegate(Chunk newChunk);
    /// <summary>
    /// This event is called after loading a chunk. All arguments are initialized and the list is already updated.
    /// </summary>
    public event ChunkLoadingDelegate OnChunkLoading = default!;
}