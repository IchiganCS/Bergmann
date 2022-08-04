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
            if (ch is null) {
                int x = 3;
            }
            else
                Chunks.AddOrReplace(ch);
        });
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
    public static int LoadDistance {
        get => _LoadDistance;
        set {
            _LoadDistance = value;
            LoadOffsets = Geometry.GetNearChunkColumns(value);
        }
    }
    private static int _LoadDistance;

    private static IEnumerable<Vector3i> LoadOffsets { get; set; }
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
            lock (RequestedColumns) {
                Vector3i currentPos = (Vector3i)getPosition();
                currentPos.Y = 0;
                IEnumerable<long> chunks = LoadOffsets
                    .Select(x => x + currentPos)
                    .Where(x => !RequestedColumns.ContainsKey(Chunk.ComputeKey(x)))
                    .SelectMany(x => {
                        int height = 5;
                        Vector3i[] vecs = new Vector3i[height];
                        RequestedColumns.TryAdd(Chunk.ComputeKey(x), 1);
                        for (int i = 0; i < height; i++) {
                            vecs[i] = new();
                            vecs[i].X = x.X;
                            vecs[i].Y = i * 16;
                            vecs[i].Z = x.Z;
                        }
                        return vecs; })
                    .Select(Chunk.ComputeKey)
                    .ToArray();

                //only request those chunks which weren't requested in the last frame
                foreach (long key in chunks) {
                    if (Data.Chunks.TryGet(key) is null) {
                        Hubs.World?.SendAsync(Names.Server.RequestChunk, key);
                    }
                }
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