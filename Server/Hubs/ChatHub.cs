using Bergmann.Shared.Networking;
using Microsoft.AspNetCore.SignalR;

namespace Bergmann.Server.Hubs;

public class ChatHub : Hub {
    [HubMethodName(Names.Server.SendMessage)]
    public async Task SendMessage(string user, string message) {
        await Clients.All.SendAsync(Names.Client.ReceiveMessage, user, message);
    }
}