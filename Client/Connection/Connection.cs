using Bergmann.Shared;
using Bergmann.Shared.Networking;
using Bergmann.Shared.Objects;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenTK.Mathematics;

namespace Bergmann.Client.Connectors;


public class Connection {


    private static Connection? _Active;
    public static Connection? Active {
        get => _Active;
        set {
            if (value?.Link == _Active?.Link || value is null)
                return;


            _Active = value;
            OnActiveChanged?.Invoke(_Active);
        }
    }


    private HubConnection WorldHub { get; set; }
    private HubConnection ChatHub { get; set; }
    public ChunkCollection Chunks { get; private set; }

    public string Link { get; private set; }

    public bool IsAlive =>
        WorldHub.State == HubConnectionState.Connected && ChatHub.State == HubConnectionState.Connected;


    public delegate void ActiveChangedDelegate(Connection newHubs);
    public static event ActiveChangedDelegate OnActiveChanged = default!;


    /// <param name="link">Without any trailing slashes, the full protocol, domain and port,
    /// e.g. http://localhost:23156</param>
    public Connection(string link) {
        Link = link;
        Logger.Info("Connecting to " + Link);

        HubConnection buildHub(string hubName) {
            HubConnection hc = new HubConnectionBuilder()
                .WithUrl($"{Link}/{hubName}")
                .WithAutomaticReconnect()
                .AddMessagePackProtocol(options => {
                    options.SerializerOptions =
                        MessagePackSerializerOptions.Standard
                        .WithResolver(new CustomResolver())
                        .WithSecurity(MessagePackSecurity.UntrustedData);
                })
                .Build();

            hc.StartAsync();
            return hc;
        }

        WorldHub = buildHub(Names.WorldHub);
        ChatHub = buildHub(Names.ChatHub);

        Chunks = new();

        WorldHub.On<Chunk>(Names.Client.ReceiveChunk, ch => {
            Chunks.AddOrReplace(ch);
        });

        WorldHub.On<long, IList<Vector3i>, IList<Block>>(Names.Client.ReceiveChunkUpdate, (ch, pos, bl) => {
            int len = Math.Min(pos.Count, bl.Count);
            if (len != pos.Count || len != bl.Count) {
                Logger.Warn($"Lengths didn't match for {Names.Client.ReceiveChunkUpdate}");
            }
            for (int i = 0; i < len; i++)
                Chunks.SetBlockAt(pos[i], bl[i]);
        });
    }

    public void RequestColumns(IEnumerable<long> keys) {
        foreach (long key in keys)
            WorldHub.SendAsync(Names.Server.RequestChunkColumn, key);
    }

    public void SendChatMessage(string sender, string message) {
        ChatHub.SendAsync(Names.Server.SendMessage, sender, message);
    }

    public void DestroyBlock(Vector3 position, Vector3 forward) {
        WorldHub.SendAsync(Names.Server.DestroyBlock, position, forward);
    }
}