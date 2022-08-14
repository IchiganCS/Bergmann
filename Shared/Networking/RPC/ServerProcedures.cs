using Microsoft.AspNetCore.SignalR.Client;
using OpenTK.Mathematics;

namespace Bergmann.Shared.Networking.RPC;


/// <summary>
/// All methods which can be invoked on the server from the client.
/// </summary>
public class ServerProcedures {

    private HubConnection Hub { get; init; }

    public ServerProcedures(HubConnection connection) {
        Hub = connection;
    }

    /// <summary>
    /// Tell the server that a chat message was sent.
    /// </summary>
    /// <param name="username">The name of the user who sent the message.</param>
    /// <param name="message">The text of the message.</param>
    public delegate void ChatMessageSentDelegate(string username, string message);
    public ChatMessageSentDelegate SendChatMessage() {
        return async (a, b) =>
            await Hub.SendAsync(nameof(SendChatMessage), a, b);
    }

    /// <summary>
    /// Tell the server that a specific client wants all information for a specific client.
    /// </summary>
    /// <param name="key">The key of the requested chunk.</param>
    public delegate void RequestChunkDelegate(long key);
    public RequestChunkDelegate SendRequestChunk() {
        return async (a) =>
            await Hub.SendAsync(nameof(SendRequestChunk), a);
    }

    /// <summary>
    /// Requestes all chunks from a specific column. The response uses the 
    /// standard <see cref="RequestChunkDelegate"/>.
    /// </summary>
    /// <param name="key">The key to any chunk in the column.</param>
    public delegate void RequestChunkColumnDelegate(long key);
    public RequestChunkColumnDelegate SendRequestChunkColumn() {
        return async (a) =>
            await Hub.SendAsync(nameof(SendRequestChunkColumn), a);
    }

    /// <summary>
    /// A client request to destroy a specific block on a server. It is given by the position of the player 
    /// and the direction to look at.
    /// </summary>
    /// <param name="position">The position of the player while executing the destruction of nature.</param>
    /// <param name="direction">The direction in which the player is looking</param>
    public delegate void DestroyBlockDelegate(Vector3 position, Vector3 direction);
    public DestroyBlockDelegate SendDestroyBlock() {
        return async (a, b) =>
            await Hub.SendAsync(nameof(SendDestroyBlock), a, b);
    }


    public delegate void PlaceBlockDelegate(Vector3 position, Vector3 direction);
    public PlaceBlockDelegate SendPlaceBlock() {
        return async (a, b) =>
            await Hub.SendAsync(nameof(SendPlaceBlock), a, b);
    }
}