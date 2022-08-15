using OpenTK.Mathematics;

namespace Bergmann.Shared.Networking;

[MessagePack.MessagePackObject]
public class DestroyBlockMessage : IMessage {

    public Vector3 Position { get; set; }
    public Vector3 Forward { get; set; }

    public DestroyBlockMessage(Vector3 position, Vector3 forward) {
        Position = position;
        Forward = forward;
    }
}