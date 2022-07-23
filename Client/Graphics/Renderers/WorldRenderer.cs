using System.Collections.Concurrent;
using Bergmann.Shared;
using Bergmann.Shared.World;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

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
    /// The connection to a WorldHub of a server.
    /// </summary>
    /// <value></value>
    private HubConnection Hub { get; set; }

    /// <summary>
    /// Constructs a world renderer for the <see cref="World.Instance"/>. It loads <see cref="ChunkRenderer"/> for
    /// every already instantiated chunk.
    /// </summary>
    public WorldRenderer() {
        ChunkRenderers = new();
        var builder = new HubConnectionBuilder()
            .WithUrl(Client.ServerAddress + "WorldHub")
            .AddMessagePackProtocol()
            .WithAutomaticReconnect();
        

        Hub = builder.Build();

        Hub.On<(int[][][], long)>("ReceiveChunk", x => {
            Task.Run(() => NewChunkRenderer(new Chunk() { Blocks = x.Item1, Key = x.Item2 }));
        });

        Hub.StartAsync();
    }

    /// <summary>
    /// Load a new chunk renderer for a given chunk and dispose of the old one.
    /// </summary>
    /// <param name="newChunk">The chunk in whose renderer's generation we're interested in</param>
    private void NewChunkRenderer(Chunk newChunk) {
        if (ChunkRenderers.ContainsKey(newChunk.Key))
            return;

        ChunkRenderer n = new(newChunk);

        ChunkRenderers.AddOrUpdate(newChunk.Key, n, (a, b) => b);
    }

    private ConcurrentBag<long> KeysToHandle { get; set; } = new();

    public void AddChunks(IEnumerable<long> keys) {
        foreach (long key in keys) {
            if (KeysToHandle.Contains(key) || ChunkRenderers.ContainsKey(key))
                continue;

            KeysToHandle.Add(key);
            Hub.SendAsync("RequestChunk", key);
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