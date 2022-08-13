using Bergmann.Shared.Objects;
using Microsoft.AspNetCore.SignalR;
using OpenTK.Mathematics;

public class ClientProcedures {


    public delegate void ChatMessageReceivedDelegate(string username, string message);
    public event ChatMessageReceivedDelegate OnChatMessageReceived = delegate { };
    public ChatMessageReceivedDelegate InvokeChatMessageReceived => OnChatMessageReceived.Invoke;
    public static ChatMessageReceivedDelegate SendMessageReceived(IClientProxy clients) {
        return async (a, b) =>
            await clients.SendAsync(nameof(OnChatMessageReceived), a, b);
    }


    public delegate void ChunkReceivedDelegate(Chunk chunk);
    public event ChunkReceivedDelegate OnChunkReceived = delegate { };
    public ChunkReceivedDelegate InvokeChunkReceived => OnChunkReceived.Invoke;
    public static ChunkReceivedDelegate SendChunkReceived(IClientProxy clients) {
        return async (chunk) =>
            await clients.SendAsync(nameof(OnChunkReceived), chunk);
    }


    public delegate void ChunkUpdateDelegate(long key, IList<Vector3i> positions, IList<Block> blocks);
    public event ChunkUpdateDelegate OnChunkUpdate = delegate { };
    public ChunkUpdateDelegate InvokeChunkUpdate => OnChunkUpdate.Invoke;
    public static ChunkUpdateDelegate SendChunkUpdate(IClientProxy clients) {
        return async (a, b, c) =>
            await clients.SendAsync(nameof(OnChunkUpdate), a, b, c);
    }
}