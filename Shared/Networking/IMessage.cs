using Bergmann.Shared.Networking.Client;
using Bergmann.Shared.Networking.Server;
using MessagePack;

namespace Bergmann.Shared.Networking.Messages;

[Union(0, typeof(ChatMessageSentMessage))]
[Union(1, typeof(ChunkColumnRequestMessage))]
[Union(2, typeof(BlockPlacementMessage))]
[Union(3, typeof(BlockDestructionMessage))]
[Union(4, typeof(RawChunkMessage))]
[Union(5, typeof(ChunkUpdateMessage))]
[Union(6, typeof(LogInAttemptMessage))]
[Union(7, typeof(SuccessfulLoginMessage))]
[Union(8, typeof(ChatMessageReceivedMessage))]
/// <summary>
/// Every message sent to and from the server is an <see cref="IMessage"/>. This enables unified handling of 
/// all messages and minimize the impact of SignalR. Because interfaces can't be sent directly, there's a wrapper
/// see both message box classes.
/// </summary>
public interface IMessage {
    
}