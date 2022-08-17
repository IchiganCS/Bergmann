using Bergmann.Shared.Objects;
using OpenTK.Mathematics;

namespace Bergmann.Shared.Networking;

[MessagePack.MessagePackObject]
/// <summary>
/// Is sent when a chunk updates. The client may not send this message, since the client may only request things.
/// The server may inform the client through this method. This message is always sent to every client.
/// The client may discard this message, if it does not care about the given chunk.
/// </summary>
public class ChunkUpdateMessage : IMessage {

    /// <summary>
    /// The key to the modified chunk.
    /// </summary>
    public long Key { get; set; }

    /// <summary>
    /// A pair of blocks and its position. The position is already in world space. The second item in the tuple
    /// is the new block to be placed at the position.
    /// </summary>
    public IList<(Vector3i, Block)> UpdatedBlocks { get; set; }


    public ChunkUpdateMessage(long key, IList<(Vector3i, Block)> updatedBlocks) {
        Key = key;
        UpdatedBlocks = updatedBlocks;
    }

    public ChunkUpdateMessage(long key, Vector3i singlePosition, Block singleBlock) :
        this(key, new List<(Vector3i, Block)>() { (singlePosition, singleBlock) }) {

    }
}