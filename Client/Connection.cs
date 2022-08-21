using Bergmann.Shared;
using Bergmann.Shared.Networking;
using Bergmann.Shared.Networking.Client;
using Bergmann.Shared.Networking.Messages;
using Bergmann.Shared.Networking.Resolvers;
using Bergmann.Shared.Networking.Server;
using Bergmann.Shared.Objects;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bergmann.Client;

/// <summary>
/// Abstracts all hub connections so that hubs can be completely removed in the entire other code.
/// A connection is built from a link, hubs are established and whenever a component wants to request an action
/// from the server, connection shall expose a method for it.
/// </summary>
public class Connection : IDisposable, IMessageHandler<SuccessfulLoginMessage> {

    /// <summary>
    /// Backing field for <see cref="Active"/>.
    /// </summary>
    private static Connection _Active = null!;

    /// <summary>
    /// The currently active connection. All components shall work with the currently active connection.
    /// </summary>
    public static Connection Active {
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
    /// The guid of the logged in user. 
    /// </summary>
    public Guid? UserID { get; private set; }

    public string? UserName { get; private set; }

    /// <summary>
    /// The chunks loaded and requested from the connection. It is held up to date by a <see cref="ChunkLoaderController"/>.
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

    static Connection() {
        StaticCompositeResolver.Instance.Register(
            GeneratedResolver.Instance,
            OpenTKResolver.Instance,
            StandardResolver.Instance
        );
    }

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

        Hub = buildHub("Hub").Result;

        RegisterMessageHandler(this);

        Chunks = new();

        Hub.On<ServerMessageBox>("ServerToClient", box => HandleServerToClient(box.Message));
    }

    private Dictionary<Type, IList<object>> MessageHandlers { get; set; } = new();


    public void RegisterMessageHandler<T>(IMessageHandler<T> handler) where T : IMessage {
        if (MessageHandlers.ContainsKey(typeof(T)))
            MessageHandlers[typeof(T)].Add(handler);
        else
            MessageHandlers.Add(typeof(T), new List<object>() { handler });
    }

    public void DropMessageHandler<T>(IMessageHandler<T> handler) where T : IMessage {
        lock (MessageHandlers)
            if (MessageHandlers.TryGetValue(typeof(T), out var value))
                value.Remove(handler);
    }

    private IEnumerable<IMessageHandler<T>> GetHandlers<T>() where T : IMessage {
        if (!MessageHandlers.ContainsKey(typeof(T)))
            return Enumerable.Empty<IMessageHandler<T>>();

        lock (MessageHandlers)
            return MessageHandlers[typeof(T)].Cast<IMessageHandler<T>>().ToArray();
    }

    public async Task Send(IMessage message) {
        await Hub.SendAsync("ClientToServer", new ClientMessageBox(message, UserID));
    }

    private void HandleServerToClient(IMessage message) {
        if (message is ChatMessageSentMessage cm)
            foreach (var item in GetHandlers<ChatMessageSentMessage>()) {
                item.HandleMessage(cm);
            }

        else if (message is ChunkColumnRequestMessage ccrm)
            foreach (var item in GetHandlers<ChunkColumnRequestMessage>()) {
                item.HandleMessage(ccrm);
            }

        else if (message is RawChunkMessage rcm)
            foreach (var item in GetHandlers<RawChunkMessage>()) {
                item.HandleMessage(rcm);
            }

        else if (message is ChunkUpdateMessage cum)
            foreach (var item in GetHandlers<ChunkUpdateMessage>()) {
                item.HandleMessage(cum);
            }

        else if (message is ChatMessageReceivedMessage crm)
            foreach (var item in GetHandlers<ChatMessageReceivedMessage>()) {
                item.HandleMessage(crm);
            }

        else if (message is SuccessfulLoginMessage slm)
            foreach (var item in GetHandlers<SuccessfulLoginMessage>()) {
                item.HandleMessage(slm);
            }

        else
            Logger.Warn("Received invalid message type");
    }


    public void Dispose() {
        Hub.DisposeAsync();
    }

    public void HandleMessage(SuccessfulLoginMessage message) {
        UserID = message.UserID;
        UserName = message.Name;
    }
}