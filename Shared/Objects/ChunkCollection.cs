using System.Collections;
using OpenTK.Mathematics;

namespace Bergmann.Shared.Objects;


/// <summary>
/// Represents any collection of chunks. One can iterate over it.
/// </summary>
public sealed class ChunkCollection : IEnumerable<Chunk> {

    /// <summary>
    /// All chunks held in the collection.
    /// </summary>
    private SortedList<long, Chunk> Chunks { get; init; }

    /// <summary>
    /// Constructs a new chunk collection with no chunks held.
    /// </summary>
    public ChunkCollection() {
        Chunks = new();
    }

    /// <summary>
    /// Adds a new chunk to the collection. If the chunk is not new, false is returned and the action does nothing.
    /// </summary>
    /// <param name="newChunk">The new chunk to be added</param>
    /// <returns>Whether the collection has been modified in any way.</returns>
    public bool Add(Chunk newChunk) {
        if (Chunks.ContainsKey(newChunk.Key))
            return false;

        Chunks.Add(newChunk.Key, newChunk);
        OnChunkAdded?.Invoke(newChunk);
        return true;
    }

    /// <summary>
    /// Updates the chunk. The chunk to be set replaced is identified by the key of the submitted chunk.
    /// The method does nothing if there is no chunk to update.
    /// </summary>
    /// <param name="newChunk">The new chunk to be held in the collection.</param>
    /// <returns>Whether any update has been made to the collection.</returns>
    public bool Replace(Chunk newChunk) {
        if (!Chunks.ContainsKey(newChunk.Key))
            return false;

        Remove(newChunk.Key);
        Add(newChunk);
        return true;
    }

    /// <summary>
    /// Adds the chunk to the collection or updates the already exisiting chunk if necessary.
    /// </summary>
    /// <param name="newChunk">The new chunk. It is guaranteed to be in the collection at the end.</param>
    public void AddOrReplace(Chunk newChunk) {
        if (!Add(newChunk))
            Replace(newChunk);
    }


    /// <summary>
    /// Removes a chunk from the collection.
    /// </summary>
    /// <param name="key">The key of the chunk to be removed.</param>
    /// <returns>Whether a chunk was removed. False if the chunk with the specified key isn't held.</returns>
    public bool Remove(long key) {
        bool res = Chunks.Remove(key, out Chunk? val);
        if (res)
            OnChunkRemoved?.Invoke(val!);

        return res;
    }

    /// <summary>
    /// Gets a chunk with the specified key.
    /// </summary>
    /// <param name="key">The key of the chunk.</param>
    /// <returns>The chunk which is stored. Can be null if no result was found.</returns>
    public Chunk? Get(long key) {
        if (Chunks.ContainsKey(key))
            return Chunks[key];

        return null;
    }



    /// <summary>
    /// Whenever a chunk is added. The new chunk is already contained in the collection.
    /// </summary>
    public event ChunkAddedDelegate OnChunkAdded = default!;
    public delegate void ChunkAddedDelegate(Chunk newChunk);

    
    /// <summary>
    /// When a chunk is only partially changed. The position of the update are also supplied.
    /// </summary>
    public event ChunkChangedDelegate OnChunkChanged = default!;
    public delegate void ChunkChangedDelegate(Chunk changedChunk, IList<Vector3i> changedPositions);


    /// <summary>
    /// When a chunk is removed. The event is only called when the chunk was already removed from
    /// the collection.
    /// </summary>
    public event ChunkRemovedDelegate OnChunkRemoved = default!;
    public delegate void ChunkRemovedDelegate(Chunk oldChunk);




    /// <summary>
    /// Enumerates over all held chunks.
    /// </summary>
    /// <returns>The chunks held.</returns>
    public IEnumerator<Chunk> GetEnumerator()
        => Chunks.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => Chunks.Values.GetEnumerator();






    /// <summary>
    /// Gets the block at a given position.
    /// </summary>
    /// <param name="collection"></param>
    /// <param name="position">The position in world space of the block.</param>
    /// <returns>The block at the given position. 0 (= air) if a chunk with the position is not held.</returns>
    public Block GetBlockAt(Vector3i position) {
        Chunk? owner = Get(Chunk.ComputeKey(position));
        if (owner is null)
            return 0;

        return owner.GetBlockWorld(position);
    }

    /// <summary>
    /// Sets a block in the collection.
    /// </summary>
    /// <param name="position">The position of the block.</param>
    /// <param name="block">The block to be set.</param>
    /// <returns>true if the operation was successful, false if the operation could not be executed.</returns>
    public bool SetBlockAt(Vector3i position, Block block) {
        long key = Chunk.ComputeKey(position);
        Chunk? owner = Get(key);
        if (owner is null)
            return false;

        owner.SetBlockWorld(position, block);
        OnChunkChanged?.Invoke(owner, new List<Vector3i>() { position });
        return true;
    }
}