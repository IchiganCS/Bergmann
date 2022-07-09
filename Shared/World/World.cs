namespace Bergmann.Shared.World;

public class World {
    public static int Distance { get; private set; } = 5;
    public static int ColumnHeight { get; private set; } = 2;

    /// <summary>
    /// Holds Distance * Distance elements. To get a chunk at offset (n, ..., m), access n * Distance + m.
    /// The elements of the list are a vertical list (bottom to top) of a chunk column.
    /// </summary>
    public LinkedList<Chunk[]> Chunks { get; set; }

    public World() {
        Chunks = new();
    }

    public void InitChunks() {       

        for (int i = 0; i < Distance * Distance; i++) {
            int x = i / Distance, z = i % Distance;
            Chunks.AddLast(LoadChunkColumn(x, z));
        }
    }

    private Chunk[] LoadChunkColumn(int x, int z) {

        Chunk[] column = new Chunk[ColumnHeight];
        for (int i = 0; i < ColumnHeight; i++) {
            column[i] = new() { Offset = new(x * 16, i * 16, z * 16) };
            OnChunkLoading?.Invoke(column[i], x, i, z);
        }

        return column;
    }

    public delegate void ChunkLoadingDelegate(Chunk newChunk, int x, int y, int z);
    public event ChunkLoadingDelegate OnChunkLoading;
}