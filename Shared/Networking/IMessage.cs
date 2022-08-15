using MessagePack;

namespace Bergmann.Shared.Networking;

[Union(0, typeof(ChatMessage))]
[Union(1, typeof(ChunkColumnRequestMessage))]
[Union(2, typeof(BlockPlacementMessage))]
[Union(3, typeof(BlockDestructionMessage))]
[Union(4, typeof(RawChunkMessage))]
[Union(5, typeof(ChunkUpdateMessage))]
public interface IMessage {
    
}