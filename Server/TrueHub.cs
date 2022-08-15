using Bergmann.Server.Handlers;
using Bergmann.Shared.Networking;
using Microsoft.AspNetCore.SignalR;

namespace Bergmann.Server;


public class TrueHub : Hub {
    private static ChatHandler Chat { get; set; } = new();
    private static WorldHandler World { get; set; } = new();

    public void ClientToServer(MessageBox box) {
        Console.WriteLine("invoked");
        if (box.Message is ChatMessage cm)
            Chat.HandleMessage(cm);

        if (box.Message is ChunkColumnRequestMessage ccrm)
            World.HandleMessage(ccrm);
    }

    // public void ClientToServer(ChatMessage message) {
    //     Chat.HandleMessage(message);
    // }
}