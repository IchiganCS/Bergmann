using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace Bergmann.Shared.World;

/// <summary>
/// This Chunk holds a three dimensional integer array (<see cref="int"/> = <see cref="Block"/>) of height, width and depth of <see cref="Chunk.CHUNK_SIZE"/>.
/// Chunks exist because handling single blocks is hard to do and inefficient in some ways (rendering for example). However, since this disables single access to blocks,
/// chunks can only be updated overall. They can of course try to retain the advantage of updating only single blocks. 
/// Effectively, chunks are a constant manager for a number of blocks. 
/// </summary>
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
    /// Since Blocks has coordinates relative to this chunk's origin, we need a way to transform it
    /// to world space.
    /// </summary>
    /// <value></value>
    public Vector3i Offset {
        get => ComputeOffset(Key);
        set => Key = ComputeKey(value);
    }

    /// <summary>
    /// Returns a number unique to this chunk and is solely dependent on the offset. Can be used as a key in a dictionary for example. 
    /// The key is calculated using <see cref="ComputeKey"/>. The key is a tightly packed array of the offsets in world chunk space as shorts
    /// </summary>
    public long Key { get; set; }


    /// <summary>
    /// Calculates the key for a chunk given a position in that chunk.
    /// </summary>
    /// <param name="position">Any block position; the returned key is the key to the chunk which holds position</param>
    /// <returns>The key to the chunk</returns>
    public static long ComputeKey(Vector3i position) {
        Span<short> span = stackalloc short[4];
        var (x, y, z) = (Vector3)position / 16f;
        span[0] = (short)Math.Floor(x);
        span[1] = (short)Math.Floor(y);
        span[2] = (short)Math.Floor(z);
        return MemoryMarshal.Cast<short, long>(span)[0];
    }

    /// <summary>
    /// Calculates the offset from a given key.
    /// </summary>
    /// <param name="key">The key of the chunk</param>
    /// <returns>The base offset for the chunk in world space</returns>
    public static Vector3i ComputeOffset(long key) {
        Span<long> span = stackalloc long[1];
        span[0] = key;
        Span<short> shorts = MemoryMarshal.Cast<long, short>(span);
        Vector3i result = new();
        result.X = shorts[0];
        result.Y = shorts[1];
        result.Z = shorts[2];
        return result * 16;
    }


    public Chunk() {
        
    }


    /// <summary>
    /// Gets a list of every block in the chunk. Since this is a three dimensional pass, this 
    /// can be quite expensive.
    /// </summary>
    /// <returns>A list filled in no particular order in chunk space</returns>
    public List<Vector3i> EveryBlock() {
        List<Vector3i> ls = new(CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE);
        for (int i = 0; i < CHUNK_SIZE; i++)
            for (int j = 0; j < CHUNK_SIZE; j++)
                for (int k = 0; k < CHUNK_SIZE; k++)
                    if (Blocks[i, j, k] != 0)
                        ls.Add(new(i, j, k));

        return ls;
    }

    /// <summary>
    /// Checks whether the block has a neighbor. At the chunk edges, false is returned
    /// </summary>
    /// <param name="position">The block whose neighbors are checked</param>
    /// <param name="direction">The direction relative to the block where there is a neighbor</param>
    /// <returns></returns>
    public bool HasNeighborAt(Vector3i position, Block.Face direction) {
        var (x, y, z) = position + Block.FaceToVector[(int)direction];

        //Chunk border
        if (x < 0 || x > CHUNK_SIZE - 1 ||
            y < 0 || y > CHUNK_SIZE - 1 ||
            z < 0 || z > CHUNK_SIZE - 1)
            return false; //TODO

        return Blocks[x, y, z] != 0;
    }

    /// <summary>
    /// The world wrapper for <see cref="GetBlockLocal"/>. Read its documentation, but replace chunk space by world space, 
    /// e.g. the position is understood to be a world space position.
    /// </summary>
    public Block GetBlockWorld(Vector3i position)
        => GetBlockLocal(position - Offset);

    /// <summary>
    /// Gets a block in this chunk at position. Position is understood to be relative to this chunk, e.g. values greater than 16 don't make sense
    /// </summary>
    /// <param name="position">A vector in chunk space</param>
    /// <returns>The block at position</returns>
    public Block GetBlockLocal(Vector3i position) {

        var (x, y, z) = position;

        if (x < 0 || x > CHUNK_SIZE - 1 ||
            y < 0 || y > CHUNK_SIZE - 1 ||
            z < 0 || z > CHUNK_SIZE - 1)
            return 0; //TODO

        return Blocks[x, y, z];
    }

    /// <summary>
    /// The world wrapper for <see cref="SetBlockLocal"/>. Read its documentation, but replace chunk space by world space, 
    /// e.g. the position is understood to be a world space position.
    /// </summary>
    public void SetBlockWorld(Vector3i position, Block block)
        => SetBlockLocal(position - Offset, block);

    /// <summary>
    /// Sets a block in this chunk at position. Position is understood to be relative to this chunk, e.g. values greater than 16 don't make sense
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
        OnUpdate?.Invoke(new() { position });
    }

    /// <summary>
    /// The delegate which is called when this chunk updates. It may be called unnecessarily sometimes, ensure that your implementation is independent of the number
    /// of calls
    /// </summary>
    /// <param name="positions">The positions of each block that has changed. Either through replacment, deletion or any other update. 
    /// Only includes those blocks that directly change. If the game logic dictates to update neighboring blocks for example, then those are passed as well.
    /// If that is not the case, then the callee is responsible to update additional information. The positions are in chunk space.</param>
    public delegate void UpdateDelegate(List<Vector3i> positions);
    /// <summary>
    /// Called whenever any block changes states or the blocks itself change.
    /// </summary>
    public event UpdateDelegate OnUpdate = default!;
}