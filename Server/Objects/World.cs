using Bergmann.Shared.Networking.Client;
using Bergmann.Shared.Networking.Messages;
using Bergmann.Shared.Networking.Server;
using Bergmann.Shared.Objects;
using Microsoft.AspNetCore.SignalR;
using OpenTK.Mathematics;

namespace Bergmann.Server.Objects;

public class World {

    private ChunkCollection Chunks { get; set; } = new();
    

    private async Task SendChunkTo(IClientProxy clients, long key) {
        Chunk? chunk = Chunks.TryGet(key);

        if (chunk is null) {
            //try generating the chunk
            chunk = Data.WorldGen.GenerateChunk(key);
            if (chunk is null)
                return;

            Chunks.Add(chunk);
        }

        await Server.Send(clients, new RawChunkMessage(chunk));
    }

    public async Task HandleMessage(IHubCallerClients clients, BlockPlacementMessage bpm) {
        if (Chunks.Raycast(bpm.Position, bpm.Forward, out Vector3i blockPos, out Geometry.Face hitFace, out _)) {
            blockPos = blockPos + Geometry.FaceToVector[(int)hitFace];
            if (Chunks.GetBlockAt(blockPos) != 0)
                return;

            long key = Chunk.ComputeKey(blockPos);
            Chunks.SetBlockAt(blockPos, 1);
            await Server.Send(clients.All, new ChunkUpdateMessage(key, blockPos, bpm.BlockToPlace));
        }
    }

    public async Task HandleMessage(IHubCallerClients clients, ChunkColumnRequestMessage ccrm) {
        Vector3i lowestChunk = Chunk.ComputeOffset(ccrm.Key);

        for (int i = 0; i < 5; i++) {
            lowestChunk.Y = i * 16;
            await SendChunkTo(clients.Caller, Chunk.ComputeKey(lowestChunk));
        }
    }

    public async Task HandleMessage(IHubCallerClients clients, BlockDestructionMessage bdm) {
        if (Chunks.Raycast(bdm.Position, bdm.Forward, out Vector3i blockPos, out _, out _)) {
            long key = Chunk.ComputeKey(blockPos);
            Chunks.SetBlockAt(blockPos, 0);
            await Server.Send(clients.All, new ChunkUpdateMessage(key, blockPos, 0));
        }
    }
}