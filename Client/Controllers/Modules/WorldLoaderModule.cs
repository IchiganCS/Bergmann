using Bergmann.Shared;
using Bergmann.Shared.Networking;
using Bergmann.Shared.Networking.Client;
using Bergmann.Shared.Networking.Messages;
using Bergmann.Shared.Networking.Server;
using Bergmann.Shared.Objects;
using OpenTK.Mathematics;

namespace Bergmann.Client.Controllers.Modules;


/// <summary>
/// Loads chunks from a connection. This is done automatically by supplying a refernce to the position
/// of preferably the player. Chunks around this position are automatically dropped and loaded as needed.
/// </summary>
public class WorldLoaderModule : Module, IDisposable,
    IMessageHandler<RawChunkMessage>,
    IMessageHandler<ChunkUpdateMessage> {

    /// <summary>
    /// The timer responsible to load chunks. It checks against a given position whether any chunks are in <see cref="LoadDistance"/>
    /// and are not loaded. If that is the case, those chunks are requested from the server.
    /// </summary>
    private Timer LoadTimer { get; set; }

    /// <summary>
    /// The timer responsible to drop chunks. If a chunk distance exceeds <see cref="DropDistance"/> but is still loaded,
    /// it is dropped.
    /// </summary>
    private Timer DropTimer { get; set; }

    /// <summary>
    /// The distance of chunks which shall be ensured to be loaded. Can be set dynamically.
    /// </summary>
    public int LoadDistance {
        get => _LoadDistance;
        set {
            _LoadDistance = value;
            LoadOffsets = Geometry.GetNearChunkColumns(value);
        }
    }
    private int _LoadDistance;


    /// <summary>
    /// This stores an offset to each chunk column around 0. Add the position to it to see which columns should be requested.
    /// Since this is solely dependent on the <see cref="LoadDistance"/> and the operation to calculate those offsets is quite expensive,
    /// the value is cached.
    /// </summary>
    private IEnumerable<Vector3i> LoadOffsets { get; set; }

    /// <summary>
    /// A list of those chunk columns which were requested. It stores the lowest chunk of each requested column.
    /// The key is the key to that chunk, the value is ignored. It updates automatically when new chunk columns are requested and when old
    /// columns are dropped.
    /// </summary>
    private SortedList<long, long> RequestedColumns { get; set; } = new();

    /// <summary>
    /// The maximal distance at which chunks should be kept in memory. If they exceed this distance, they are dropped.
    /// </summary>
    public int DropDistance { get; set; }


    /// <summary>
    /// Generates timers to load and drop chunks in the given intervals using <see cref="LoadDistance"/> and 
    /// <see cref="DropDistance"/>.
    /// </summary>
    /// <param name="dropDistance">The distance at which chunks are dropped from memory</param>
    /// <param name="loadDistance">The distance at which chunks are loaded from the server</param>
    /// <param name="loadTime">The interval in which loading required chunks are loaded.</param>
    /// <param name="dropTime">The interval in which chunks out of reach are dropped.</param>
    public WorldLoaderModule(int loadTime = 250, int dropTime = 2000,
        int loadDistance = 10, int dropDistance = 40) {

        LoadOffsets = Enumerable.Empty<Vector3i>();

        LoadDistance = loadDistance;
        DropDistance = dropDistance;

        LoadTimer = new(async x => {
            if (!IsActive)
                return;


            // it contains only chunks which were not previously. It only stores one chunk per column
            IEnumerable<long> chunks;
            lock (RequestedColumns) {
                Vector3i currentPos = (Vector3i)(Parent as GameController)!.Fph.Position;
                currentPos.Y = 0;
                chunks = LoadOffsets
                    .Select(x => Chunk.ComputeKey(x + currentPos))
                    .Where(x => !RequestedColumns.ContainsKey(x))
                    .Select(key => {
                        try {
                            RequestedColumns.TryAdd(key, key);
                            return key;
                        } catch (Exception e) {
                            Logger.Warn("Received error:\n" + e.Message);
                            return key;
                        }
                    })
                    .ToArray();
            }

            foreach (long chunk in chunks)
                await Connection.Active.SendAsync(new ChunkColumnRequestMessage(chunk));

        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(loadTime));

        DropTimer = new(x => {
            if (!IsActive)
                return;

            Connection.Active.Chunks.ForEach(chunk => {
                if ((chunk.Offset.Xz - (Parent as GameController)!.Fph.Position.Xz).LengthFast > DropDistance * 16) {
                    Vector3i offset = chunk.Offset;
                    offset.Y = 0;
                    RequestedColumns.Remove(Chunk.ComputeKey(offset));
                    Connection.Active.Chunks.Remove(chunk.Key);
                }
            });
        }, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(dropTime));
    }

    /// <summary>
    /// Add a chunk update to the global list of updates.
    /// </summary>
    public void HandleMessage(ChunkUpdateMessage message) {
        foreach (var block in message.UpdatedBlocks) {
            Connection.Active.Chunks.SetBlockAt(block.Item1, block.Item2);
        }
    }

    /// <summary>
    /// Add the entirely new chunk to the global collection.
    /// </summary>
    public void HandleMessage(RawChunkMessage message) {
        Connection.Active.Chunks.AddOrReplace(message.Chunk);
    }

    public override void OnActivated(Controller controller) {
        base.OnActivated(controller);
        Connection.Active.RegisterMessageHandler<RawChunkMessage>(this);
        Connection.Active.RegisterMessageHandler<ChunkUpdateMessage>(this);
    }

    public override void OnDeactivated() {
        base.OnDeactivated();
        Connection.Active.DropMessageHandler<RawChunkMessage>(this);
        Connection.Active.DropMessageHandler<ChunkUpdateMessage>(this);
    }

    /// <summary>
    /// This should be called to make sure that the timers are disposed of in a controlled manner.
    /// Not calling this method does not leak memory, but the timers are running as long as 
    /// they are not garbage collected.
    /// </summary>
    public void Dispose() {
        LoadTimer.Dispose();
        DropTimer.Dispose();
    }
}