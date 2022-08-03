using Bergmann.Shared;
using Bergmann.Shared.Networking;
using Bergmann.Shared.Objects;
using Microsoft.AspNetCore.SignalR.Client;
using OpenTK.Mathematics;

namespace Bergmann.Client;

/// <summary>
/// Holds all data from the client, e.g. user settings or loaded chunks.
/// </summary>
public static class Data {
    /// <summary>
    /// The chunks held by the current client. It is always up to date, one may subscribe to its events.
    /// </summary>
    /// <returns></returns>
    public static ChunkCollection Chunks { get; set; } = new();

    /// <summary>
    /// Connects some always required functionality from the hubs to the appropriate properties like
    /// <see cref="Chunks"/>.
    /// </summary>
    public static void MakeHubConnections() {
        // if (!Hubs.ConnectionAlive) {
        //     Logger.Warn($"Couldn't establish hub connections for {nameof(Data)}");
        //     return;
        // }


        Hubs.World?.On<Chunk>(Names.Client.ReceiveChunk, Chunks.AddOrReplace);
        Hubs.World?.On<long, IList<Vector3i>, IList<Block>>(Names.Client.ReceiveChunkUpdate, (ch, pos, bl) => {
            int len = Math.Min(pos.Count, bl.Count);
            for (int i = 0; i < len; i++)
                Chunks.SetBlockAt(pos[i], bl[i]);
        });
    }





    /// <summary>
    /// The timer responsible to load chunks. It checks against a given position whether any chunks are in <see cref="LoadDistance"/>
    /// and are not loaded. If that is the case, those chunks are requested from the server.
    /// </summary>
    private static Timer? LoadTimer { get; set; }

    /// <summary>
    /// The timer responsible to drop chunks. If a chunk distance exceeds <see cref="DropDistance"/> but is still loaded,
    /// it is dropped.
    /// </summary>
    private static Timer? DropTimer { get; set; }

    /// <summary>
    /// The distance of chunks which shall be ensured to be loaded. Can be set dynamically.
    /// </summary>
    public static int LoadDistance { get; set; } = 6;

    /// <summary>
    /// The maximal distance at which chunks should be kept in memory. If they exceed this distance, they are dropped.
    /// </summary>
    public static int DropDistance { get; set; } = 20;


    /// <summary>
    /// A helper variable: It caches all chunks which were requested in the previous frame to stop flooding the server
    /// and processing the same chunk multiple times.
    /// </summary>
    /// <typeparam name="long">The keys of the chunks</typeparam>
    private static IEnumerable<long> PreviouslyRequestedChunks { get; set; } = Array.Empty<long>();


    /// <summary>
    /// Generates timers to load and drop chunks in the given intervals using <see cref="LoadDistance"/> and 
    /// <see cref="DropDistance"/>. <paramref name="getPosition"/> is a function to get the current position of the player.
    /// </summary>
    /// <param name="getPosition">A function which always returns the correct position of the player.</param>
    /// <param name="loadTime">The interval in which loading required chunks are loaded.</param>
    /// <param name="dropTime">The interval in which chunks out of reach are dropped.</param>
    public static void SubscribeToPositionUpdate(Func<Vector3> getPosition, int loadTime = 250, int dropTime = 2000) {

        LoadTimer = new(x => {
            IEnumerable<long> chunks = Geometry.GetNearChunks(getPosition(), LoadDistance);
            IEnumerable<long> diff = chunks.Where(x => !PreviouslyRequestedChunks.Contains(x)).ToArray();
            PreviouslyRequestedChunks = chunks;

            //only request those chunks which weren't requested in the last frame
            foreach (long key in diff) {
                if (Data.Chunks.Get(key) is null)
                    Hubs.World?.SendAsync(Names.Server.RequestChunk, key);
            }
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(loadTime));

        DropTimer = new(x => {
            foreach (Chunk chunk in Data.Chunks.ToArray()) {
                if ((chunk.Offset - getPosition()).LengthFast > DropDistance * 16) {

                    Data.Chunks.Remove(chunk.Key);
                }
            }
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(dropTime));
    }
}