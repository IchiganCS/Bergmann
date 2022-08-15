
namespace Bergmann.Shared.Networking;

[MessagePack.MessagePackObject]
public class ChunkColumnRequestMessage : IRequestMessage {
    public long Key { get; set; }

    public string ConnectionId { get; init; }

    public ChunkColumnRequestMessage(string connectionId, long key) {
        ConnectionId = connectionId;
        Key = key;
    }
}