using Bergmann.Shared.Networking.Messages;

namespace Bergmann.Shared.Networking;

[MessagePack.MessagePackObject]
/// <summary>
/// A wrapper for a <see cref="IMessage"/>. This object can be sent directly to the server and client.
/// It does not server any purpose but better serialization support. MessagePack struggles with deserializing
/// raw interfaces (currently).
/// </summary>
public class ClientMessageBox {

    /// <summary>
    /// The boxed message.
    /// </summary>
    public IMessage Message { get; set; }

    /// <summary>
    /// The connection id of the client sending the message.
    /// </summary>
    public string ConnectionId { get; }

    /// <summary>
    /// Constructs a new box for the message.
    /// </summary>
    /// <param name="message">The message to be wrapped.</param>
    public ClientMessageBox(IMessage message, string connectionId) {
        Message = message;
        ConnectionId = connectionId;
    }
}