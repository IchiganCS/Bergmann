using System.Reflection;
using Bergmann.Shared;
using Bergmann.Shared.Networking;
using Bergmann.Shared.Objects;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using OpenTK.Mathematics;

namespace Bergmann.Client.Connectors;

/// <summary>
/// Abstracts all hub connections so that hubs can be completely removed in the entire other code.
/// A connection is built from a link, hubs are established and whenever a component wants to request an action
/// from the server, connection shall expose a method for it.
/// </summary>
public class Connection : IDisposable {

    /// <summary>
    /// Backing field for <see cref="Active"/>.
    /// </summary>
    private static Connection? _Active;

    /// <summary>
    /// The currently active connection. All components shall work with the currently active connection.
    /// </summary>
    public static Connection? Active {
        get => _Active;
        set {
            if (value?.Link == _Active?.Link || value is null)
                return;


            _Active = value;
            OnActiveChanged?.Invoke(_Active);
        }
    }

    /// <summary>
    /// A world hub, an instance of the class from the server package.
    /// </summary>
    private HubConnection WorldHub { get; init; }
    /// <summary>
    /// A chat hub, an instance of the class from the server package.
    /// </summary>
    private HubConnection ChatHub { get; init; }

    /// <summary>
    /// The chunks loaded and requested from the connection. It is held up to date by a <see cref="ChunkLoader"/>.
    /// </summary>
    public ChunkCollection Chunks { get; init; }

    /// <summary>
    /// The link used to establish the connection.
    /// </summary>
    /// <value></value>
    public Uri Link { get; init; }

    /// <summary>
    /// Whether the connection is currently connected to the server with all hubs.
    /// </summary>
    public bool IsAlive =>
        WorldHub.State == HubConnectionState.Connected && ChatHub.State == HubConnectionState.Connected;


    public delegate void ActiveChangedDelegate(Connection newHubs);

    /// <summary>
    /// Invoked when the currently active connection changed. The new connection is already active when this event is invoked.
    /// </summary>
    public static event ActiveChangedDelegate OnActiveChanged = default!;

    /// <summary>
    /// Builds a new connection and builds required hubs and other initialization.
    /// </summary>
    /// <param name="link">Without any trailing slashes, the full protocol, domain and port,
    /// e.g. http://localhost:23156</param>
    public Connection(string link) {
        Link = new(link);
        Logger.Info("Connecting to " + Link);

        async Task<HubConnection> buildHub(string hubName) {
            HubConnection hc = new HubConnectionBuilder()
                .WithUrl(new Uri(Link, hubName))
                .WithAutomaticReconnect()
                .AddMessagePackProtocol(options => {
                    options.SerializerOptions =
                        MessagePackSerializerOptions.Standard
                        .WithResolver(new CustomResolver())
                        .WithSecurity(MessagePackSecurity.UntrustedData);
                })
                .Build();

            await hc.StartAsync();
            return hc;
        }

        WorldHub = buildHub(Names.WorldHub).Result;
        ChatHub = buildHub(Names.ChatHub).Result;

        Chunks = new();
        Client = new();


        foreach (var ev in Client.GetType().GetEvents()) {
            MethodInfo invokeMethod = ev.EventHandlerType!.GetMethod("Invoke")!;
            PropertyInfo invokeProperty = Client.GetType().GetProperties().First(x => x.PropertyType == ev.EventHandlerType);
            MethodInfo getInvoker = invokeProperty.GetMethod!;
            Type[] paramterTypes = invokeMethod.GetParameters().Select(x => x.ParameterType).ToArray();

            switch (ev.Name) {
                case nameof(ClientProcedures.OnChatMessageReceived):
                    ChatHub.On<string, string>(ev.Name, 
                        (a, b) => Client.InvokeChatMessageReceived(a, b));
                    break;
                case nameof(ClientProcedures.OnChunkReceived):
                    WorldHub.On<Chunk>(ev.Name, 
                        (a) => Client.InvokeChunkReceived(a));
                    break;
                case nameof(ClientProcedures.OnChunkUpdate):
                    WorldHub.On<long, IList<Vector3i>, IList<Block>>(ev.Name, 
                        (a, b, c) => Client.InvokeChunkUpdate(a, b, c));
                    break;
                default:

                    ChatHub.On(ev.Name,
                        paramterTypes,
                        (args) => {
                            return Task.Run(() => {
                                (getInvoker.Invoke(Client, null) as Delegate)!.DynamicInvoke(args);
                            });
                        }
                    );
                    WorldHub.On(ev.Name,
                        paramterTypes,
                        (args) => {
                            return Task.Run(() => {
                                (getInvoker.Invoke(Client, null) as Delegate)!.DynamicInvoke(args);
                            });
                        }
                    );
                    break;
            }
        }


        Client.OnChatMessageReceived += (user, message) => {
            Console.WriteLine($"user {user} wrote {message}");
        };


        Client.OnChunkUpdate += (ch, pos, bl) => {
            int len = Math.Min(pos.Count, bl.Count);
            if (len != pos.Count || len != bl.Count) {
                Logger.Warn($"Lengths didn't match for {Names.Client.ReceiveChunkUpdate}");
            }
            for (int i = 0; i < len; i++)
                Chunks.SetBlockAt(pos[i], bl[i]);
        };

        Client.OnChunkReceived += ch => {
            Chunks.AddOrReplace(ch);
        };
    }

    public ClientProcedures Client { get; private set; }

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


    public void Dispose() {
        WorldHub.DisposeAsync();
        ChatHub.DisposeAsync();
    }
}