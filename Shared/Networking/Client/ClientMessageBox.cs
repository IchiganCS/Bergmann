using Bergmann.Shared.Networking.Messages;

namespace Bergmann.Shared.Networking.Client;

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
    public IMessage Message { get; init; }

    /// <summary>
    /// The guid of the logged in user.
    /// </summary>
    public Guid? Guid { get; init; }

    /// <summary>
    /// Constructs a new box for the message.
    /// </summary>
    /// <param name="message">The message to be wrapped.</param>
    /// <param name="guid">The guid of the logged in user. If the value is null, the server may only execute actions
    /// wihout a login.</param>
    public ClientMessageBox(IMessage message, Guid? guid) {
        Message = message;
        Guid = guid;
    }
}