using Microsoft.AspNetCore.SignalR;

namespace Bergmann.Server.Hubs;

public class WorldHub : Hub {
    public async void RequestChunk(long key) {
        Data.World.LoadChunk(key);
        if (Data.World.Chunks.ContainsKey(key))
            await Clients.Caller.SendAsync("ReceiveChunk", (Data.World.Chunks[key].Blocks, key));
    }
}