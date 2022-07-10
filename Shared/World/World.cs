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


    /// <summary>
    /// The delegate for the corresponding event
    /// </summary>
    /// <param name="newChunk">Chunk is fully initialized and already inserted</param>
    public delegate void ChunkLoadingDelegate(Chunk newChunk);
    /// <summary>
    /// This event is called after loading a chunk. All arguments are initialized and the list is already updated.
    /// </summary>
    public event ChunkLoadingDelegate OnChunkLoading;
}