using Bergmann.Shared.Networking;
using Bergmann.Shared.Networking.RPC;
using Bergmann.Shared.Objects;
using Microsoft.AspNetCore.SignalR;
using OpenTK.Mathematics;

namespace Bergmann.Server;

[MatchDelegateChecker(typeof(TrueHub))]
public class TrueHub : Hub {
    [MatchDelegate(typeof(ServerProcedures.ChatMessageSentDelegate))]
    [HubMethodName(nameof(ServerProcedures.SendChatMessage))]
    public void SendMessage(string user, string message) {
        Console.WriteLine("received message");
        ClientProcedures.SendMessageReceived(Clients.All)(user, message);
    }


    /// <summary>
    /// Called from each callee - the server responds with an appropriate call to <see cref="Names.ReceiveChunk"/> if
    /// deemed possible. If a key can't loaded and not generated, then no response will follow.
    /// </summary>
    /// <param name="key">The key of the chunk to be loaded.</param>
    [MatchDelegate(typeof(ServerProcedures.RequestChunkDelegate))]
    [HubMethodName(nameof(ServerProcedures.SendRequestChunk))]
    public void RequestChunk(long key) {
        Chunk? chunk = Data.World.Chunks.TryGet(key);

        if (chunk is null) {
            //try generating the chunk
            chunk = Data.WorldGen.GenerateChunk(key);
            if (chunk is null)
                return;

            Data.World.Chunks.Add(chunk);
        }

        ClientProcedures.SendChunkReceived(Clients.Caller)(chunk);
    }


    /// <summary>
    /// Requestes all chunks from a specific column. The response uses the standard <see cref="Names.Client.ReceiveChunk"/>.
    /// </summary>
    /// <param name="key">The key to any chunk in the column.</param>
    [MatchDelegate(typeof(ServerProcedures.RequestChunkColumnDelegate))]
    [HubMethodName(nameof(ServerProcedures.SendRequestChunkColumn))]
    public void RequestChunkColumn(long key) {
        Vector3i lowestChunk = Chunk.ComputeOffset(key);

        for (int i = 0; i < 5; i++) {
            lowestChunk.Y = i * 16;
            RequestChunk(Chunk.ComputeKey(lowestChunk));
        }
    }


    /// <summary>
    /// A client request to destroy a specific block on a server. It is given by the position of the player 
    /// and the direction to look at.
    /// </summary>
    /// <param name="position">The position of the player while executing the destruction of nature.</param>
    /// <param name="direction">The direction in which the player is looking</param>
    [MatchDelegate(typeof(ServerProcedures.DestroyBlockDelegate))]
    [HubMethodName(nameof(ServerProcedures.SendDestroyBlock))]
    public void DestroyBlock(Vector3 position, Vector3 direction) {
        if (Data.World.Chunks.Raycast(position, direction, out Vector3i blockPos, out _)) {
            long key = Chunk.ComputeKey(blockPos);
            Data.World.Chunks.SetBlockAt(blockPos, 0);
            ClientProcedures.SendChunkUpdate(Clients.All)
                (key, new List<Vector3i>() { blockPos }, new List<Block>() { 0 });
        }
    }


    [MatchDelegate(typeof(ServerProcedures.PlaceBlockDelegate))]
    [HubMethodName(nameof(ServerProcedures.SendPlaceBlock))]
    public void PlaceBlock(Vector3 position, Vector3 direction) {
        if (Data.World.Chunks.Raycast(position, direction, out Vector3i blockPos, out Geometry.Face hitFace)) {
            blockPos = blockPos + Geometry.FaceToVector[(int)hitFace];
            if (Data.World.Chunks.GetBlockAt(blockPos) != 0)
                return;

            long key = Chunk.ComputeKey(blockPos);
            Data.World.Chunks.SetBlockAt(blockPos, 1);
            ClientProcedures.SendChunkUpdate(Clients.All)
                (key, new List<Vector3i>() { blockPos }, new List<Block>() { 1 });
        }
    }
}