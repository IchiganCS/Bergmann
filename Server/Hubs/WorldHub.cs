using Bergmann.Shared.World;
using Microsoft.AspNetCore.SignalR;

namespace Bergmann.Server.Hubs;

public class WorldHub : Hub {
    private World Instance { get; set; } = new();


}