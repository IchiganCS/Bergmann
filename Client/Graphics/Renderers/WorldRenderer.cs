using Bergmann.Client.Graphics.Renderers.Passers;
using Bergmann.Shared.Objects;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// A renderer for the entire world. Of course, it doesn't render the entire world, but only handles
/// the different render passes. It holds a set of <see cref="IRendererPasser"/> to achieve this.
/// </summary>
public class WorldRenderer : IDisposable {

    /// <summary>
    /// A list of all active render passers.
    /// </summary>
    private IList<IRendererPasser> RenderPassers { get; set; }

    /// <summary>
    /// Adds a given chunk to all render passers.
    /// </summary>
    /// <param name="key">The key to the chunk.</param>
    private void AddChunkToAll(long key) {
        foreach (IRendererPasser passer in RenderPassers)
            passer.AddChunk(key);
    }

    private void AddChunkToAllIfAroundLoaded(long key)
        => AddChunkToAllIfAroundLoaded(Chunk.ComputeOffset(key), key);

    private void AddChunkToAllIfAroundLoaded(Vector3i offset, long key) {
        if (Connection.Active!.Chunks.Any(x => x.Key == key) &&            
            Geometry.AllFaces.Select(x => Geometry.FaceToVector[(int)x] * 16 + offset)
            .Where(x => x.Y > 0) // since only those cold be existent
            .Select(Chunk.ComputeKey)
            .All(y => Connection.Active!.Chunks.Any(x => x.Key == y)))

            AddChunkToAll(key);
    }

    private void DropChunkToAll(long key) {
        foreach (IRendererPasser passer in RenderPassers)
            passer.DropChunk(key);
    }

    public WorldRenderer() {
        RenderPassers = new List<IRendererPasser>() {
            new SolidsPasser()
        };

        Connection.Active!.Chunks.OnChunkChanged += OnChunkUpdated;
        Connection.Active!.Chunks.OnChunkAdded += OnChunkAdded;
        Connection.Active!.Chunks.OnChunkRemoved += OnChunkRemoved;

        Connection.Active!.Chunks.ForEach(x => 
            AddChunkToAllIfAroundLoaded(x.Key));
    }

    private void OnChunkUpdated(Chunk chunk, IList<Vector3i> positions) {
        List<long> keys = new();

        AddChunkToAllIfAroundLoaded(chunk.Key);
        if (positions.Any(x => (x - chunk.Offset).Y == 0))
            keys.Add(Chunk.ComputeKey(chunk.Offset - (0, 16, 0)));
        if (positions.Any(x => (x - chunk.Offset).Y == 15))
            keys.Add(Chunk.ComputeKey(chunk.Offset + (0, 16, 0)));
        if (positions.Any(x => (x - chunk.Offset).X == 0))
            keys.Add(Chunk.ComputeKey(chunk.Offset - (16, 0, 0)));
        if (positions.Any(x => (x - chunk.Offset).X == 15))
            keys.Add(Chunk.ComputeKey(chunk.Offset + (16, 0, 0)));
        if (positions.Any(x => (x - chunk.Offset).Z == 0))
            keys.Add(Chunk.ComputeKey(chunk.Offset - (0, 0, 16)));
        if (positions.Any(x => (x - chunk.Offset).Z == 15))
            keys.Add(Chunk.ComputeKey(chunk.Offset + (0, 0, 16)));

        foreach (var key in keys)
            AddChunkToAllIfAroundLoaded(key);
    }

    private void OnChunkAdded(Chunk chunk) {
        // only load chunks were all neighbors are loaded in the non-rendered chunk list.
        // Then we don't have to deal with the situation that chunk faces are loaded which don't need to be actually rendered.
        // If all around are loaded, one can safely assume that each lookup on the neighbors succeeds and we don't have
        // to deal with illegetimate assumptions which would result in a necessity to reload later.

        AddChunkToAllIfAroundLoaded(chunk.Offset, chunk.Key);

        foreach (var neighborOffset in Geometry.AllFaces.Select(x => Geometry.FaceToVector[(int)x] * 16 + chunk.Offset))
            AddChunkToAllIfAroundLoaded(neighborOffset, Chunk.ComputeKey(neighborOffset));
    }

    private void OnChunkRemoved(Chunk chunk)
        => DropChunkToAll(chunk.Key);


    public void Render(Frustum box) {
        foreach (IRendererPasser passer in RenderPassers)
            passer.Render(box);
    }


    public void Dispose() {
        Connection.Active!.Chunks.OnChunkChanged -= OnChunkUpdated;
        Connection.Active!.Chunks.OnChunkAdded -= OnChunkAdded;
        Connection.Active!.Chunks.OnChunkRemoved -= OnChunkRemoved;

        foreach (IRendererPasser passer in RenderPassers)
            passer.Dispose();

        RenderPassers.Clear();
    }
}