using Bergmann.Shared.Objects;
using Microsoft.AspNetCore.SignalR;
using OpenTK.Mathematics;

namespace Bergmann.Shared.Networking.RPC;

/// <summary>
/// A unified way to make method calls from the server to the client.
/// Adding new methods can be easily done, follow the given samples.
/// The names of the rpc calls used behind the scenes is the name of the specified event.
/// </summary>
public class ClientProcedures {

    /// <summary>
    /// The client receives a message to display.
    /// </summary>
    /// <param name="username">The name of the user who sent the message.</param>
    /// <param name="message">The text of the message.</param>
    public delegate void ChatMessageReceivedDelegate(string username, string message);
    public event ChatMessageReceivedDelegate OnChatMessageReceived = delegate { };
    public ChatMessageReceivedDelegate InvokeChatMessageReceived => OnChatMessageReceived.Invoke;
    public static ChatMessageReceivedDelegate SendMessageReceived(IClientProxy clients) {
        return async (a, b) =>
            await clients.SendAsync(nameof(OnChatMessageReceived), a, b);
    }


    /// <summary>
    /// The client receives a chunk and loads its graphical properties etc.
    /// </summary>
    /// <param name="chunk">The entire chunk to be sent to the client.</param>
    public delegate void ChunkReceivedDelegate(Chunk chunk);
    public event ChunkReceivedDelegate OnChunkReceived = delegate { };
    public ChunkReceivedDelegate InvokeChunkReceived => OnChunkReceived.Invoke;
    public static ChunkReceivedDelegate SendChunkReceived(IClientProxy clients) {
        return async (chunk) =>
            await clients.SendAsync(nameof(OnChunkReceived), chunk);
    }


    /// <summary>
    /// The client receives information to update a block, it may or may not decline the information, dependent on whether
    /// the entire chunk was already loaded.
    /// </summary>
    /// <param name="key">The key of the updated chunk.</param>
    /// <param name="positions">The positions of the blocks which were updated.</param>
    /// <param name="blocks">The blocks to be set. This list has the same length as positions.</param>
    public delegate void ChunkUpdateDelegate(long key, IList<Vector3i> positions, IList<Block> blocks);
    public event ChunkUpdateDelegate OnChunkUpdate = delegate { };
    public ChunkUpdateDelegate InvokeChunkUpdate => OnChunkUpdate.Invoke;
    public static ChunkUpdateDelegate SendChunkUpdate(IClientProxy clients) {
        return async (a, b, c) =>
            await clients.SendAsync(nameof(OnChunkUpdate), a, b, c);
    }
}