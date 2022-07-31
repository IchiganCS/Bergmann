using Bergmann.Shared.Networking;
using Bergmann.Shared.World;
using Microsoft.AspNetCore.SignalR.Client;
using OpenTK.Mathematics;

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
    private SortedDictionary<long, ChunkRenderer> ChunkRenderers { get; set; }

    /// <summary>
    /// The count of loaded chunks.
    /// </summary>
    public int ChunkCount => ChunkRenderers.Count;


    /// <summary>
    /// Constructs a world renderer for the <see cref="World.Instance"/>. It subscribes to updates from the world hub
    /// for chunk receiving and updating. It currently doesn't support loading chunks on startup, it is recommended to
    /// call this method before working on chunks.
    /// </summary>
    public WorldRenderer() {
        ChunkRenderers = new();

        Hubs.World?.On<Chunk>(Names.ReceiveChunk, chunk => {
            lock (ChunkRenderers) {
                if (ChunkRenderers.ContainsKey(chunk.Key)) {
                    Task.Run(() => {
                        bool res = ChunkRenderers.TryGetValue(chunk.Key, out ChunkRenderer? ch);
                        
                        if (res)
                            Task.Run(() => ch?.Update(chunk));
                    });
                }
                else {
                    ChunkRenderer renderer = new();
                    Task.Run(() => renderer.Update(chunk));
                    ChunkRenderers.Add(chunk.Key, renderer);
                }
            }
        });
    }


    /// <summary>
    /// The timer responsible to load chunks. It checks against a given position whether any chunks are in <see cref="LoadDistance"/>
    /// and are not loaded. If that is the case, those chunks are requested from the server.
    /// </summary>
    private Timer? LoadTimer { get; set; }

    /// <summary>
    /// The timer responsible to drop chunks. If a chunk distance exceeds <see cref="DropDistance"/> but is still loaded,
    /// it is dropped.
    /// </summary>
    private Timer? DropTimer { get; set; }

    /// <summary>
    /// The distance of chunks which shall be ensured to be loaded. Can be set dynamically.
    /// </summary>
    public int LoadDistance { get; set; } = 6;

    /// <summary>
    /// The maximal distance at which chunks should be kept in memory. If they exceed this distance, they are dropped.
    /// </summary>
    public int DropDistance { get; set; } = 20;


    /// <summary>
    /// A helper variable: It caches all chunks which were requested in the previous frame to stop flooding the server
    /// and processing the same chunk multiple times.
    /// </summary>
    /// <typeparam name="long">The keys of the chunks</typeparam>
    private IEnumerable<long> PreviouslyRequestedChunks { get; set; } = Array.Empty<long>();


    /// <summary>
    /// Generates timers to load and drop chunks in the given intervals using <see cref="LoadDistance"/> and 
    /// <see cref="DropDistance"/>. <paramref name="getPosition"/> is a function to get the current position of the player.
    /// </summary>
    /// <param name="getPosition">A function which always returns the correct position of the player.</param>
    /// <param name="loadTime">The interval in which loading required chunks are loaded.</param>
    /// <param name="dropTime">The interval in which chunks out of reach are dropped.</param>
    public void SubscribeToPositionUpdate(Func<Vector3> getPosition, int loadTime = 250, int dropTime = 2000) {

        LoadTimer = new(x => {
            IEnumerable<long> chunks = World.GetNearChunks(getPosition(), LoadDistance);
            IEnumerable<long> diff = chunks.Where(x => !PreviouslyRequestedChunks.Contains(x)).ToArray();
            PreviouslyRequestedChunks = chunks;

            //only request those chunks which weren't requested in the last frame
            foreach (long key in diff) {
                if (!ChunkRenderers.ContainsKey(key))
                    Hubs.World?.SendAsync(Names.RequestChunk, key);
            }
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(loadTime));

        DropTimer = new(x => {
            ChunkRenderer[] renderers;

            lock (ChunkRenderers)
                renderers = ChunkRenderers.Values.ToArray();

            foreach (ChunkRenderer chunkRenderer in renderers) {
                if ((Chunk.ComputeOffset(chunkRenderer.ChunkKey) - getPosition()).LengthFast > DropDistance * 16) {

                    lock (ChunkRenderers) {
                        ChunkRenderers.Remove(chunkRenderer.ChunkKey);
                        GlThread.Invoke(chunkRenderer.Dispose);
                    }
                }
            }
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(dropTime));
    }


    /// <summary>
    /// Calls <see cref="ChunkRenderer.Render"/> for each chunk renderer held
    /// </summary>
    public void Render() {
        lock (ChunkRenderers) {
            foreach (ChunkRenderer cr in ChunkRenderers.Values) {
                cr.Render();
            }
        }
    }

    /// <summary>
    /// Disposes of all held chunk renderers and clears the list.
    /// </summary>
    public void Dispose() {
        lock (ChunkRenderers) {
            foreach (ChunkRenderer cr in ChunkRenderers.Values)
                cr.Dispose();

            PreviouslyRequestedChunks = Array.Empty<long>();
            ChunkRenderers.Clear();
        }
    }
}