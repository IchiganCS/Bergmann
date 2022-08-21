using Bergmann.Shared.Networking.Messages;
using Bergmann.Shared.Objects;
using OpenTK.Mathematics;

namespace Bergmann.Shared.Networking.Client;

[MessagePack.MessagePackObject]
/// <summary>
/// The client asks the server to place a block from the given rotation and position.
/// The server then performs a raycast and places the block.
/// </summary>
public class BlockPlacementMessage : IMessage {

    /// <summary>
    /// The position of the placer; required to perform the raycast.
    /// </summary>
    public Vector3 Position { get; set; }

    /// <summary>
    /// The direction in which the placer is looking.
    /// </summary>
    public Vector3 Forward { get; set; }

    /// <summary>
    /// The block to be added to the world.
    /// </summary>
    public Block BlockToPlace { get; set; }
    public BlockPlacementMessage(Vector3 position, Vector3 forward, Block blockToPlace) {
        Position = position;
        Forward = forward;
        BlockToPlace = blockToPlace;
    }
}