using Bergmann.Shared.Objects;

namespace Bergmann.Shared.Networking;

[MessagePack.MessagePackObject]
public class RawChunkMessage : IMessage {
    public Chunk Chunk { get; set; }

    public RawChunkMessage(Chunk chunk) {
        Chunk = chunk;
    }
}