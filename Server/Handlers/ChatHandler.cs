using Bergmann.Shared.Networking;

namespace Bergmann.Server.Handlers;


public class ChatHandler : IMessageHandler<ChatMessage> {
    private List<ChatMessage> Messages { get; set; } = new();

    public async void HandleMessage(ChatMessage message) {
        Messages.Add(message);
        Console.WriteLine($"user {message.Sender} wrote {message.Text}");

        await Server.SendToClientAsync(Server.Clients.All, message);
    }
}