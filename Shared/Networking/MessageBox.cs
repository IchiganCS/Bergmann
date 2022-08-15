namespace Bergmann.Shared.Networking;

[MessagePack.MessagePackObject]
public class MessageBox {

    public IMessage Message { get; set; }

    public MessageBox(IMessage message) {
        Message = message;
    }
}