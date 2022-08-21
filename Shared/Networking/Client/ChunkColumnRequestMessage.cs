using Bergmann.Shared.Networking.Messages;

namespace Bergmann.Shared.Networking.Client;

[MessagePack.MessagePackObject]
/// <summary>
/// The client might use this type of message to request a entire chunk column from the server. This proves useful
/// for easier loading and gives more control possibilities to the server.
/// </summary>
public class ChunkColumnRequestMessage : IMessage {
    /// <summary>
    /// The key to any chunk in the column.
    /// </summary>
    public long Key { get; set; }

    
    public ChunkColumnRequestMessage(long key) {
        Key = key;
    }
}