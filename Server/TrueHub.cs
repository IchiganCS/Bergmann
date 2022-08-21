using Bergmann.Server.Objects;
using Bergmann.Shared.Networking;
using Bergmann.Shared.Networking.Messages;
using Bergmann.Shared.Objects;
using Microsoft.AspNetCore.SignalR;
using OpenTK.Mathematics;

namespace Bergmann.Server;


public class TrueHub : Hub {
    private static async Task Send(IClientProxy clients, IMessage message)
        => await Server.SendToClientAsync(clients, message);

    private static async Task SendChunkTo(IClientProxy clients, long key) {
        Chunk? chunk = Data.World.Chunks.TryGet(key);

        if (chunk is null) {
            //try generating the chunk
            chunk = Data.WorldGen.GenerateChunk(key);
            if (chunk is null)
                return;

            Data.World.Chunks.Add(chunk);
        }

        await Send(clients, new RawChunkMessage(chunk));
    }

    public async void ClientToServer(ClientMessageBox box) {
        if (box.Message is ChatMessage cm) {
            Data.ChatMessages.Add(cm);
            Console.WriteLine($"user {cm.Sender} wrote {cm.Text}");

            await Send(Clients.All, cm);
        }

        if (box.Message is ChunkColumnRequestMessage ccrm) {
            Vector3i lowestChunk = Chunk.ComputeOffset(ccrm.Key);

            for (int i = 0; i < 5; i++) {
                lowestChunk.Y = i * 16;
                await SendChunkTo(Clients.Caller, Chunk.ComputeKey(lowestChunk));
            }
        }

        if (box.Message is BlockPlacementMessage bpm) {
            if (Data.World.Chunks.Raycast(bpm.Position, bpm.Forward, out Vector3i blockPos, out Geometry.Face hitFace, out _)) {
                blockPos = blockPos + Geometry.FaceToVector[(int)hitFace];
                if (Data.World.Chunks.GetBlockAt(blockPos) != 0)
                    return;

                long key = Chunk.ComputeKey(blockPos);
                Data.World.Chunks.SetBlockAt(blockPos, 1);
                await Send(Clients.All, new ChunkUpdateMessage(key, blockPos, bpm.BlockToPlace));
            }
        }

        if (box.Message is BlockDestructionMessage bdm) {
            if (Data.World.Chunks.Raycast(bdm.Position, bdm.Forward, out Vector3i blockPos, out _, out _)) {
                long key = Chunk.ComputeKey(blockPos);
                Data.World.Chunks.SetBlockAt(blockPos, 0);
                await Send(Clients.All, new ChunkUpdateMessage(key, blockPos, 0));
            }
        }
    }
}