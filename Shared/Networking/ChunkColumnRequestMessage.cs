
namespace Bergmann.Shared.Networking;

[MessagePack.MessagePackObject]
/// <summary>
/// The client might use this type of message to request a entire chunk column from the server. This proves useful
/// for easier loading and gives more control possibilities to the server.
/// </summary>
public class ChunkColumnRequestMessage : IRequestMessage {
    /// <summary>
    /// The key to any chunk in the column.
    /// </summary>
    public long Key { get; set; }

    /// <summary>
    /// The connection id to which the chunk is to be sent.
    /// </summary>
    public string ConnectionId { get; init; }

    
    public ChunkColumnRequestMessage(string connectionId, long key) {
        ConnectionId = connectionId;
        Key = key;
    }
}