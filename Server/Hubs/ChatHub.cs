using Microsoft.AspNetCore.SignalR;

namespace Bergmann.Server.Hubs;

public class ChatHub : Hub {
    public async Task SendMessage(string user, string message) {
        await Clients.All.SendAsync("PrintMsg", user, message);
    }
}