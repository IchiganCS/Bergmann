using Microsoft.AspNetCore.SignalR;

namespace Bergmann.Shared.Networking;

public interface IRequestMessage : IMessage {
    public string ConnectionId { get; }
}