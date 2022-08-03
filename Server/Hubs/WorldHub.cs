using Bergmann.Shared.Networking;
using Bergmann.Shared.Objects;
using Microsoft.AspNetCore.SignalR;
using OpenTK.Mathematics;

namespace Bergmann.Server.Hubs;

/// <summary>
/// A synchronization place for the world the server holds. It may get a lot more functionality eventually 
/// and currently operates exclusively on <see cref="Data.World"/>. Each method name has to be annotated with
/// <see cref="HubMethodNameAttribute"/> and bound to the given entry in <see cref="Names"/>. 
/// See the already existing examples.
/// </summary>
public class WorldHub : Hub {


    /// <summary>
    /// Called from each callee - the server responds with an appropriate call to <see cref="Names.ReceiveChunk"/> if
    /// deemed possible. If a key can't loaded and not generated, then no response will follow.
    /// </summary>
    /// <param name="key">The key of the chunk to be loaded.</param>
    [HubMethodName(Names.Server.RequestChunk)]
    public async void RequestChunk(long key) {
        Chunk? chunk = Data.World.Chunks.Get(key);

        if (chunk is null) {
            //generate the chunk
            chunk = Data.WorldGen.GenerateChunk(key);


            if (chunk is null) {
                //the chunk could not be generated.
                return;
            }

            Data.World.Chunks.Add(chunk);
        }


        await Clients.Caller.SendAsync(Names.Client.ReceiveChunk, chunk);
    }


    /// <summary>
    /// A client request to destroy a specific block on a server. It is given by the position of the player 
    /// and the direction to look at.
    /// </summary>
    /// <param name="position">The position of the player while executing the destruction of nature.</param>
    /// <param name="direction">The direction in which the player is looking</param>
    [HubMethodName(Names.Server.DestroyBlock)]
    public async void DestroyBlock(Vector3 position, Vector3 direction) {
        if (Data.World.Chunks.Raycast(position, direction, out Vector3i blockPos, out _)) {
            long key = Chunk.ComputeKey(blockPos);
            Data.World.Chunks.SetBlockAt(blockPos, 0);
            await Clients.All.SendAsync(Names.Client.ReceiveChunkUpdate, 
                key, new List<Vector3i>() { blockPos }, new List<Block>() { 0 });
        }
    }

    [HubMethodName(Names.Server.DropWorld)]
    public void DropWorld() {
        Data.World.Clear();
    }
}