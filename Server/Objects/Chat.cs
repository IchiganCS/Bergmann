using Bergmann.Shared.Networking.Client;
using Bergmann.Shared.Networking.Messages;
using Bergmann.Shared.Networking.Server;
using Microsoft.AspNetCore.SignalR;

namespace Bergmann.Server.Objects;

public class Chat {
    private IList<ChatMessageSentMessage> History { get; set; } = new List<ChatMessageSentMessage>();

    /// <summary>
    /// A user controller to fetch names from messages.
    /// </summary>
    private User User { get; set; }

    public Chat(User user) {
        User = user;
    }

    public async Task HandleMessage(IHubCallerClients clients, ChatMessageSentMessage cm, Guid id) {
        History.Add(cm);
        await Server.Send(clients.All, new ChatMessageReceivedMessage(cm.Text, User.GetName(id)!));
    }
}