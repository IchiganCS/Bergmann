using Bergmann.Server.Handlers;
using Bergmann.Shared.Networking;
using Microsoft.AspNetCore.SignalR;

namespace Bergmann.Server;


public class TrueHub : Hub {
    private static ChatHandler Chat { get; set; } = new();
    private static WorldHandler World { get; set; } = new();

    public void ClientToServer(MessageBox box) {
        if (box.Message is ChatMessage cm)
            Chat.HandleMessage(cm);

        if (box.Message is ChunkColumnRequestMessage ccrm)
            World.HandleMessage(ccrm);

        if (box.Message is BlockPlacementMessage bpm)
            World.HandleMessage(bpm);

        if (box.Message is BlockDestructionMessage bdm)
            World.HandleMessage(bdm);
    }
}