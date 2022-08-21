using Bergmann.Server.Objects;
using Bergmann.Shared;
using Bergmann.Shared.Networking;
using Bergmann.Shared.Networking.Client;
using Bergmann.Shared.Networking.Messages;
using Microsoft.AspNetCore.SignalR;

namespace Bergmann.Server;


public class TrueHub : Hub {
    private static World World { get; set; } = new();
    private static User User { get; set; } = new();
    private static Chat Chat { get; set; } = new(User);



    /// <summary>
    /// This method is called from SignalR. It forwards an <see cref="IMessage"/> to an appropriate handler.
    /// </summary>
    /// <param name="box">The message box from the client.</param>
    public async void ClientToServer(ClientMessageBox box) {
        if (box.Message is LogInAttemptMessage liam) {
            await User.HandleMessage(Clients, liam);
            return;
        }

        if (box.Guid is null || !User.IsLoggedIn(box.Guid)) {
            Logger.Warn("Tried unauthorized message");
            return;
        }

        if (box.Message is ChatMessageSentMessage cm)
            await Chat.HandleMessage(Clients, cm, (Guid)box.Guid!);

        else if (box.Message is ChunkColumnRequestMessage ccrm)
            await World.HandleMessage(Clients, ccrm);

        else if (box.Message is BlockPlacementMessage bpm)
            await World.HandleMessage(Clients, bpm);

        else if (box.Message is BlockDestructionMessage bdm)
            await World.HandleMessage(Clients, bdm);


        else
            Logger.Warn("Received invalid message");
    }
}