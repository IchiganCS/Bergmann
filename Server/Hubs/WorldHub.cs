using Bergmann.Shared.Networking;
using Bergmann.Shared.World;
using Microsoft.AspNetCore.SignalR;
using OpenTK.Mathematics;

namespace Bergmann.Server.Hubs;

/// <summary>
/// A synchronization place for the world the server holds. It may get a lot more functionality eventually 
/// and currently operates exclusively on <see cref="Data.World"/>. Each method name has to be annotated with
/// <see cref="HubMethodNameAttribute"/> and bound to the given entry in <see cref="Names"/>. See the already existing to methods.
/// </summary>
public class WorldHub : Hub {


    /// <summary>
    /// Called from each callee - the server responds with an appropriate call to <see cref="Names.ReceiveChunk"/>.
    /// </summary>
    /// <param name="key">The key of the chunk to be loaded</param>
    [HubMethodName(Names.RequestChunk)]
    public async void RequestChunk(long key) {
        Data.World.LoadChunk(key);
        if (Data.World.Chunks.ContainsKey(key) && Data.World.Chunks[key].Blocks is not null) {
            await Clients.Caller.SendAsync(Names.ReceiveChunk, Data.World.Chunks[key]);
        }
    }


    /// <summary>
    /// A client request to destroy a specific block on a server. It is given by the position of the player 
    /// and the direction to look at.
    /// </summary>
    /// <param name="position">The position of the player while executing the destruction of nature.</param>
    /// <param name="direction">The direction in which the player is looking</param>
    [HubMethodName(Names.DestroyBlock)]
    public async void DestroyBlock(Vector3 position, Vector3 direction) {
        if (Data.World.Raycast(position, direction, out Vector3i blockPos, out _)) {
            long key = Chunk.ComputeKey(blockPos);
            Data.World.SetBlockAt(blockPos, 0);
            await Clients.All.SendAsync(Names.ReceiveChunk, Data.World.Chunks[key]);
        }
    }

    [HubMethodName(Names.DropWorld)]
    public void DropWorld() {
        Data.World.Chunks.Clear();
    }
}