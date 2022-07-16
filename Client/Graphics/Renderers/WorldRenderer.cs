using System.Collections.Concurrent;
using Bergmann.Shared.World;

namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// A renderer for the entire world. Of course, it doesn't render the entire world, but only handles
/// the rendering of chunks. It holds a set of <see cref="ChunkRenderer"/> which are automatically created
/// and destroyed when called for. It registers to the events of the <see cref="World"/> class to achieve this.
/// </summary>
public class WorldRenderer : IDisposable, IRenderer {

    /// <summary>
    /// The key is the <see cref="Chunk.Key"/> which is unique and fast. Make sure that when items are removed
    /// or overwritten, they are properly disposed of.
    /// </summary>
    private ConcurrentDictionary<long, ChunkRenderer> ChunkRenderers{ get; set; }

    /// <summary>
    /// Constructs a world renderer for the <see cref="World.Instance"/>. It loads <see cref="ChunkRenderer"/> for
    /// every already instantiated chunk.
    /// </summary>
    public WorldRenderer() {
        ChunkRenderers = new();

        World.Instance.OnChunkLoading += NewChunkRenderer;
        
        foreach (Chunk ch in World.Instance.Chunks.Values)
            NewChunkRenderer(ch);
    }

    /// <summary>
    /// Load a new chunk renderer for a given chunk and dispose of the old one.
    /// </summary>
    /// <param name="newChunk">The chunk in whose renderer's generation we're interested in</param>
    private void NewChunkRenderer(Chunk newChunk) {
        ChunkRenderer n = new();
        Task.Run(() => {
            n.InitWith(newChunk);
        });

        if (ChunkRenderers.ContainsKey(newChunk.Key)) {
            ChunkRenderers[newChunk.Key].Dispose();
            ChunkRenderers[newChunk.Key] = n;
        }
        else {
            ChunkRenderers.AddOrUpdate(newChunk.Key, n, (a, b) => b);
        }
    }

    /// <summary>
    /// Calls <see cref="ChunkRenderer.Render"/> for each chunk renderer held
    /// </summary>
    public void Render() {
        foreach (ChunkRenderer cr in ChunkRenderers.Values)
            cr?.Render();
    }


    public void Dispose() {
        foreach (ChunkRenderer cr in ChunkRenderers.Values)
            cr.Dispose();
    }
}