using MessagePack;

namespace Bergmann.Shared.Networking.Messages;

[Union(0, typeof(ChatMessage))]
[Union(1, typeof(ChunkColumnRequestMessage))]
[Union(2, typeof(BlockPlacementMessage))]
[Union(3, typeof(BlockDestructionMessage))]
[Union(4, typeof(RawChunkMessage))]
[Union(5, typeof(ChunkUpdateMessage))]
/// <summary>
/// Every message sent to and from the server is an <see cref="IMessage"/>. This enables unified handling of 
/// all messages and minimize the impact of SignalR. Because interfaces can't be sent directly, there's a wrapper
/// <see cref="ClientMessageBox"/>.
/// </summary>
public interface IMessage {
    
}