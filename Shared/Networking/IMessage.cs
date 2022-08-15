namespace Bergmann.Shared.Networking;

[MessagePack.Union(0, typeof(ChatMessage))]
[MessagePack.Union(1, typeof(ChunkColumnRequestMessage))]
public interface IMessage {
    
}