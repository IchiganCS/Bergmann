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
        if (!Hubs.ConnectionAlive) {
            Logger.Warn($"Couldn't establish hub connections for {nameof(Data)}");
            return;
        }


        Hubs.World?.On<Chunk>(Names.Client.ReceiveChunk, ch => {
            Chunks.AddOrReplace(ch);
        });
        Hubs.World?.On<long, IList<Vector3i>, IList<Block>>(Names.Client.ReceiveChunkUpdate, (ch, pos, bl) => {
            int len = Math.Min(pos.Count, bl.Count);
            if (len != pos.Count || len != bl.Count) {
                Logger.Warn($"Lengths didn't match for {Names.Client.ReceiveChunkUpdate}");
            }
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
    public static int LoadDistance {
        get => _LoadDistance;
        set {
            _LoadDistance = value;
            LoadOffsets = Geometry.GetNearChunkColumns(value);
        }
    }
    private static int _LoadDistance;

    /// <summary>
    /// This stores an offset to each chunk column around 0. Add the position to it to see which columns should be requested.
    /// Since this is solely dependent on the <see cref="LoadDistance"/> and the operation to calculate those offsets is quite expensive,
    /// the value is cached.
    /// </summary>
    private static IEnumerable<Vector3i>? LoadOffsets { get; set; }

    /// <summary>
    /// A list of those chunk columns which were requested. It stores the lowest chunk of each requested column.
    /// The key is the key to that chunk, the value is ignored. It updates automatically when new chunk columns are requested and when old
    /// columns are dropped.
    /// </summary>
    private static SortedList<long, long> RequestedColumns { get; set; } = new();

    /// <summary>
    /// The maximal distance at which chunks should be kept in memory. If they exceed this distance, they are dropped.
    /// </summary>
    public static int DropDistance { get; set; }


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
            if (LoadOffsets is null) {
                // cannot load any chunks if we can't calculate which ones :)
                Logger.Warn($"Tried to load chunks, but no {nameof(LoadOffsets)} were given.");
                return;
            }

            // it contains only chunks which were not previously. It only stores one chunk per column
            IEnumerable<long> chunks;
            lock (RequestedColumns) {
                Vector3i currentPos = (Vector3i)getPosition();
                currentPos.Y = 0;
                chunks = LoadOffsets
                    .Select(x => x + currentPos)
                    .Where(x => !RequestedColumns.ContainsKey(Chunk.ComputeKey(x)))
                    .Select(x => {
                        long key = Chunk.ComputeKey(x);
                        RequestedColumns.Add(key, key);
                        return key;
                    })
                    .ToArray();
            }

            foreach (long key in chunks) {
                Hubs.World?.SendAsync(Names.Server.RequestChunkColumn, key);
            }
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(loadTime));

        DropTimer = new(x => {
            Data.Chunks.ForEach(chunk => {
                if ((chunk.Offset - getPosition()).LengthFast > DropDistance * 16) {
                    Vector3i offset = chunk.Offset;
                    offset.Y = 0;
                    RequestedColumns.Remove(Chunk.ComputeKey(offset));
                    Data.Chunks.Remove(chunk.Key);
                }
            });
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(dropTime));
    }
}