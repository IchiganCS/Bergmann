using OpenTK.Mathematics;

namespace Bergmann.Shared.Networking;

[MessagePack.MessagePackObject]

/// <summary>
/// The client requests to destroy a block. Similar to <see cref="BlockPlacementMessage"/>, the client
/// only sends positional data of the actor, the server then performs a raycast.
/// </summary>
public class BlockDestructionMessage : IMessage {

    public Vector3 Position { get; set; }
    public Vector3 Forward { get; set; }

    public BlockDestructionMessage(Vector3 position, Vector3 forward) {
        Position = position;
        Forward = forward;
    }
}