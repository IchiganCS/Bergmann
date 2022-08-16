using OpenTK.Mathematics;

namespace Bergmann.Shared.Objects;

/// <summary>
/// This Chunk holds a three dimensional integer array (<see cref="int"/> = <see cref="Block"/>) of height, width 
/// and depth of <see cref="Chunk.CHUNK_SIZE"/>. Chunks exist because handling single blocks is hard to do and inefficient 
/// in some ways (rendering for example). However, since this disables single access to blocks,
/// chunks can only be updated overall. They can of course try to retain the advantage of updating only single blocks. 
/// Effectively, chunks are a constant manager for a number of blocks. 
/// </summary>
[MessagePack.MessagePackObject]
public class Chunk {
    /// <summary>
    /// It might be wise to *not* change this value. Since it's used for storing in files :)
    /// </summary>
    public const int CHUNK_SIZE = 16;

    /// <summary>
    /// The blocks this chunk holds. It is always size (<see cref="CHUNK_SIZE"/>), size, size in its dimensions.
    /// </summary>
    /// <value></value>
    public int[,,] Blocks { get; set; }

    /// <summary>
    /// Returns a number unique to this chunk and is solely dependent on the offset. Can be used as a key in a dictionary for example. 
    /// The key is calculated using <see cref="ComputeKey"/>. The key is a tightly packed array of the offsets in world chunk space as shorts
    /// </summary>
    public long Key {
        get => _Key;
        set {
            _Key = value;
            _Offset = Chunk.ComputeOffset(_Key);
        }
    }
    private long _Key = 0;

    /// <summary>
    /// Since Blocks has coordinates relative to this chunk's origin, we need a way to transform it
    /// to world space.
    /// </summary>
    [MessagePack.IgnoreMember]
    public Vector3i Offset {
        get => _Offset;
        set {
            _Offset = value;
            _Key = Chunk.ComputeKey(value);
        }
    }
    private Vector3i _Offset = new();

    /// <summary>
    /// Calculates the key for a chunk given a position in that chunk.
    /// </summary>
    /// <param name="position">Any block position; the returned key is the key to the chunk which holds position</param>
    /// <returns>The key to the chunk</returns>
    public static unsafe long ComputeKey(Vector3i position) {
        //the key is a long, separated into four shorts.
        //the first short, highest value, is unused.
        //the second highest short stores x, the third highest y
        //and the lowermost short stores z. Span<short> seems to be too slow.
        var (x, y, z) = position / 16;
        if (x * 16 > position.X)
            x--;
        if (y * 16 > position.Y)
            y--;
        if (z * 16 > position.Z)
            z--;
        long res = 0;
        short temp;

        temp = (short)x;
        res |= *(ushort*)&temp;
        res <<= sizeof(ushort) * 8;
        temp = (short)y;
        res |= *(ushort*)&temp;
        res <<= sizeof(ushort) * 8;
        temp = (short)z;
        res |= *(ushort*)&temp;
        return res;
    }

    /// <summary>
    /// Calculates the offset from a given key.
    /// </summary>
    /// <param name="key">The key of the chunk</param>
    /// <returns>The base offset for the chunk in world space</returns>
    public static unsafe Vector3i ComputeOffset(long key) {
        Vector3i result = new();
        ushort bitmask = ushort.MaxValue;
        ushort temp;

        temp = (ushort)(key & bitmask);
        result.Z = *(short*)&temp;
        key >>= sizeof(ushort) * 8;

        temp = (ushort)(key & bitmask);
        result.Y = *(short*)&temp;
        key >>= sizeof(ushort) * 8;

        temp = (ushort)(key & bitmask);
        result.X = *(short*)&temp;
        return result * 16;
    }


    /// <summary>
    /// Generates a new chunk. The intial values are nonsense, it is necessary to overwrite them.
    /// </summary>
    public Chunk() {
        Key = -1;
        Blocks = new int[0, 0, 0];
    }


    /// <summary>
    /// Executes an action for every block.
    /// </summary>
    /// <param name="action">The first paramter is the local position of the block, the second is the current block</param>
    public void ForEach(Action<Vector3i, Block> action) {
        for (int x = 0; x < CHUNK_SIZE; x++)
            for (int y = 0; y < CHUNK_SIZE; y++)
                for (int z = 0; z < CHUNK_SIZE; z++)
                    action((x, y, z), Blocks[x, y, z]);
    }


    /// <summary>
    /// The world wrapper for <see cref="GetBlockLocal"/>. Read its documentation, but replace chunk space by world space, 
    /// e.g. the position is understood to be a world space position.
    /// </summary>
    public Block GetBlockWorld(Vector3i position)
        => GetBlockLocal(position - Offset);

    /// <summary>
    /// Gets a block in this chunk at position. Position is understood to be relative to this chunk, e.g. values greater 
    /// than 16 or less than 0 don't make sense.
    /// </summary>
    /// <param name="position">A vector in chunk space.</param>
    /// <returns>The block at position. 0 if the position is out of bounds.</returns>
    public Block GetBlockLocal(Vector3i position) {

        var (x, y, z) = position;

        if (x < 0 || x > CHUNK_SIZE - 1 ||
            y < 0 || y > CHUNK_SIZE - 1 ||
            z < 0 || z > CHUNK_SIZE - 1)
            return 0;

        return Blocks[x, y, z];
    }

    /// <summary>
    /// The world wrapper for <see cref="SetBlockLocal"/>. Read its documentation, but replace chunk space by world space, 
    /// e.g. the position is understood to be a world space position.
    /// </summary>
    public void SetBlockWorld(Vector3i position, Block block)
        => SetBlockLocal(position - Offset, block);

    /// <summary>
    /// Sets a block in this chunk at position. Position is understood to be relative to this chunk, e.g. values greater 
    /// than 16 don't make sense
    /// </summary>
    /// <param name="position">A vector in chunk space</param>
    /// <param name="block">The block to be placed into the chunk</param>
    public void SetBlockLocal(Vector3i position, Block block) {
        var (x, y, z) = position;

        if (x < 0 || x > CHUNK_SIZE - 1 ||
            y < 0 || y > CHUNK_SIZE - 1 ||
            z < 0 || z > CHUNK_SIZE - 1)
            return;

        Blocks[x, y, z] = block;
    }
}