using Bergmann.Shared.Networking.Messages;

namespace Bergmann.Shared.Networking;


/// <summary>
/// A class may implement this interface if it can handle the reception of a message of <typeparamref name="T"/>.
/// Registering handlers differs between client and server. Nothing is stopping a class from implementing this interface
/// with multiple different message types.
/// </summary>
/// <typeparam name="T">Any subtype of <see cref="IMessage"/>.</typeparam>
public interface IMessageHandler<T> where T : IMessage {

    /// <summary>
    /// This method is called when a new message of the specified type was received and the handler
    /// should now do whatever it wants to do with the message.
    /// </summary>
    /// <param name="message">The message sent by the communication partner.</param>
    public void HandleMessage(T message);
}