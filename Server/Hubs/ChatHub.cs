using Bergmann.Shared.Networking;
using Microsoft.AspNetCore.SignalR;

namespace Bergmann.Server.Hubs;

public class ChatHub : Hub {
    [HubMethodName(Names.SendMessage)]
    public async Task SendMessage(string user, string message) {
        await Clients.All.SendAsync("PrintMsg", user, message);
    }
}