using System.Reflection;
using Bergmann.Shared;
using Bergmann.Shared.Networking;
using Bergmann.Shared.Networking.Resolvers;
using Bergmann.Shared.Objects;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
    private HubConnection Hub { get; init; }

    /// <summary>
    /// The connection id of the hub. Useful when requesting information from the server.
    /// </summary>
    public string ConnectionId => Hub.ConnectionId!;

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
        Hub.State == HubConnectionState.Connected;


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
                    StaticCompositeResolver.Instance.Register(
                        GeneratedResolver.Instance,
                        CustomResolver.Instance,
                        StandardResolver.Instance
                    );

                    options.SerializerOptions =
                        MessagePackSerializerOptions.Standard
                        .WithResolver(StaticCompositeResolver.Instance)
                        .WithSecurity(MessagePackSecurity.UntrustedData);
                })
                .ConfigureLogging(logging => {
                    logging.AddConsole();
                })
                .Build();

            await hc.StartAsync();
            return hc;
        }

        Hub = buildHub(Names.Hub).Result;

        Chunks = new();

        Hub.On<MessageBox>("ServerToClient", box => HandleServerToClient(box.Message));
    }

    private Dictionary<Type, IList<object>> MessageHandlers { get; set; } = new();

    public void RegisterReflectionMessageHandler(object handler) {
        foreach (Type implemented in handler.GetType().GetInterfaces()) {
            if (!implemented.IsGenericType)
                continue;

            if (implemented.GetGenericTypeDefinition() != typeof(IMessageHandler<>))
                continue;

            Type generic = implemented.GetGenericArguments()[0];

            if (MessageHandlers.ContainsKey(generic))
                MessageHandlers[generic].Add(handler);
            else
                MessageHandlers.Add(generic, new List<object>() { handler });
        }
    }

    public void RegisterMessageHandler<T> (IMessageHandler<T> handler) where T : IMessage {
        if (MessageHandlers.ContainsKey(typeof(T)))
            MessageHandlers[typeof(T)].Add(handler);
        else
            MessageHandlers.Add(typeof(T), new List<object>() { handler });
    }

    public void DropMessageHandler(object handler) {
        foreach (var entry in MessageHandlers) {
            entry.Value.Remove(handler);
        }
    }

    public void DropMessageHandler<T>(IMessageHandler<T> handler) where T : IMessage {
        if (MessageHandlers.TryGetValue(typeof(T), out var value))
            value.Remove(handler);
    }

    private IEnumerable<IMessageHandler<T>> GetHandlers<T>() where T : IMessage {
        if (!MessageHandlers.ContainsKey(typeof(T)))
            return Enumerable.Empty<IMessageHandler<T>>();

        return MessageHandlers[typeof(T)].Cast<IMessageHandler<T>>();
    }

    public async Task ClientToServerAsync(IMessage message) {
        await Hub.SendAsync("ClientToServer", MessageBox.Create(message));
    }

    private void HandleServerToClient(IMessage message) {
        if (message is ChatMessage cm)
            foreach (var item in GetHandlers<ChatMessage>()) {
                item.HandleMessage(cm);
            }

        if (message is ChunkColumnRequestMessage ccrm)
            foreach (var item in GetHandlers<ChunkColumnRequestMessage>()) {
                item.HandleMessage(ccrm);
            }

        if (message is RawChunkMessage rcm)
            foreach (var item in GetHandlers<RawChunkMessage>()) {
                item.HandleMessage(rcm);
            }

        if (message is ChunkUpdateMessage cum)
            foreach (var item in GetHandlers<ChunkUpdateMessage>()) {
                item.HandleMessage(cum);
            }
    }


    public void Dispose() {
        Hub.DisposeAsync();
    }
}