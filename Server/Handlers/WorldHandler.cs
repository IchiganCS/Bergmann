using Bergmann.Shared.Networking;
using Bergmann.Shared.Objects;
using Microsoft.AspNetCore.SignalR;
using OpenTK.Mathematics;

namespace Bergmann.Server.Handlers;

public class WorldHandler : IMessageHandler<BlockPlacementMessage>, IMessageHandler<BlockDestructionMessage>,
    IMessageHandler<ChunkColumnRequestMessage> {

    private async Task SendEntireChunkTo(IClientProxy clients, long key) {
        Chunk? chunk = Data.World.Chunks.TryGet(key);

        if (chunk is null) {
            //try generating the chunk
            chunk = Data.WorldGen.GenerateChunk(key);
            if (chunk is null)
                return;

            Data.World.Chunks.Add(chunk);
        }

        await Server.SendToClientAsync(clients, new RawChunkMessage(chunk));
    }

    public async void HandleMessage(ChunkColumnRequestMessage message) {
        Vector3i lowestChunk = Chunk.ComputeOffset(message.Key);

        for (int i = 0; i < 5; i++) {
            lowestChunk.Y = i * 16;
            await SendEntireChunkTo(Server.Clients.Client(message.ConnectionId), Chunk.ComputeKey(lowestChunk));
        }
    }

    public async void HandleMessage(BlockPlacementMessage message) {
        if (Data.World.Chunks.Raycast(message.Position, message.Forward, out Vector3i blockPos, out Geometry.Face hitFace)) {
            blockPos = blockPos + Geometry.FaceToVector[(int)hitFace];
            if (Data.World.Chunks.GetBlockAt(blockPos) != 0)
                return;

            long key = Chunk.ComputeKey(blockPos);
            Data.World.Chunks.SetBlockAt(blockPos, 1);
            await Server.SendToClientAsync(Server.Clients.All, new ChunkUpdateMessage(key, blockPos, message.BlockToPlace));
        }
    }

    public async void HandleMessage(BlockDestructionMessage message) {
        if (Data.World.Chunks.Raycast(message.Position, message.Forward, out Vector3i blockPos, out _)) {
            long key = Chunk.ComputeKey(blockPos);
            Data.World.Chunks.SetBlockAt(blockPos, 0);
            await Server.SendToClientAsync(Server.Clients.All, new ChunkUpdateMessage(key, blockPos, 0));            
        }
    }
}