using Bergmann.Shared.Objects;
using OpenTK.Mathematics;

namespace Bergmann.Shared.Networking;

[MessagePack.MessagePackObject]
public class BlockPlacementMessage : IMessage {

    public Vector3 Position { get; set; }
    public Vector3 Forward { get; set; }

    public Block BlockToPlace { get; set; }
    public BlockPlacementMessage(Vector3 position, Vector3 forward, Block blockToPlace) {
        Position = position;
        Forward = forward;
        BlockToPlace = blockToPlace;
    }
}