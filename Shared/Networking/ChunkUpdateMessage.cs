using Bergmann.Shared.Objects;
using OpenTK.Mathematics;

namespace Bergmann.Shared.Networking;

[MessagePack.MessagePackObject]
public class ChunkUpdateMessage : IMessage {

    public long Key { get; set; }
    public IList<(Vector3i, Block)> UpdatedBlocks { get; set; }


    public ChunkUpdateMessage(long key, IList<(Vector3i, Block)> updatedBlocks) {
        Key = key;
        UpdatedBlocks = updatedBlocks;
    }

    public ChunkUpdateMessage(long key, Vector3i singlePosition, Block singleBlock) :
        this(key, new List<(Vector3i, Block)>() { (singlePosition, singleBlock) }) {

    }
}