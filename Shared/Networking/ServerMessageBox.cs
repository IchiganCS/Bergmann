using Bergmann.Shared.Networking.Messages;

namespace Bergmann.Shared.Networking;

[MessagePack.MessagePackObject]
/// <summary>
/// A wrapper for a <see cref="IMessage"/>. This object can be sent directly to the server and client.
/// It does not server any purpose but better serialization support. MessagePack struggles with deserializing
/// raw interfaces (currently).
/// </summary>
public class ServerMessageBox {

    /// <summary>
    /// The boxed message.
    /// </summary>
    public IMessage Message { get; set; }

    /// <summary>
    /// Constructs a new box for the message.
    /// </summary>
    /// <param name="message">The message to be wrapped.</param>
    public ServerMessageBox(IMessage message) {
        Message = message;
    }
}