using System.Collections.Concurrent;
using Bergmann.Shared.Networking;
using Bergmann.Shared.World;
using Microsoft.AspNetCore.SignalR.Client;

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
    private ConcurrentDictionary<long, ChunkRenderer> ChunkRenderers { get; set; }


    /// <summary>
    /// Constructs a world renderer for the <see cref="World.Instance"/>. It subscribes to updates from the world hub
    /// for chunk receiving and updating. It currently doesn't support loading chunks on startup, it is recommended to
    /// call this method before working on chunks.
    /// </summary>
    public WorldRenderer() {
        ChunkRenderers = new();

        Hubs.World.On<Chunk>(Names.ReceiveChunk, chunk => {
            if (ChunkRenderers.ContainsKey(chunk.Key)) {
                Task.Run(() => ChunkRenderers[chunk.Key].Update(chunk));
            }
            else {
                ChunkRenderer renderer = new();
                Task.Run(() => renderer.Update(chunk));
                ChunkRenderers.AddOrUpdate(chunk.Key, renderer, (a, b) => renderer);
            }
        });
    }

    // Both of these attributes are yet to be implemented properly.

    private ConcurrentBag<long> KeysToHandle { get; set; } = new();

    public void AddChunks(IEnumerable<long> keys) {
        foreach (long key in keys) {
            if (KeysToHandle.Contains(key) || ChunkRenderers.ContainsKey(key))
                continue;

            KeysToHandle.Add(key);
            Hubs.World.SendAsync(Names.RequestChunk, key);
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