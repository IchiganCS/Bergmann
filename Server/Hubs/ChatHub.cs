using Bergmann.Shared.Networking;
using Microsoft.AspNetCore.SignalR;

namespace Bergmann.Server.Hubs;

public class ChatHub : Hub {
    [HubMethodName(Names.Server.SendMessage)]
    public void SendMessage(string user, string message) {
        Console.WriteLine("received message");
        ClientProcedures.SendMessageReceived(Clients.All)(user, message);
    }
}