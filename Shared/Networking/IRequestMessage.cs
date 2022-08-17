namespace Bergmann.Shared.Networking;

/// <summary>
/// This interface might be merged with message: currently, it stores an additional connection string
/// to give the server the option to only respond to the sender of the message.
/// </summary>
public interface IRequestMessage : IMessage {
    public string ConnectionId { get; }
}