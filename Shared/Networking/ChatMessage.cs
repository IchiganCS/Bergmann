namespace Bergmann.Shared.Networking;

[MessagePack.MessagePackObject]
public class ChatMessage : IMessage {
    
    public string Sender { get; set; }

    public string Text { get; set; }


    public ChatMessage(string sender, string text) {
        Sender = sender;
        Text = text;
    }
}