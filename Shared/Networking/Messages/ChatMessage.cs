namespace Bergmann.Shared.Networking.Messages;

[MessagePack.MessagePackObject]
/// <summary>
/// Bidirectional; the client might inform the server of a sent chat message by the client, the server then transfers
/// this message to every other client.
/// </summary>
public class ChatMessage : IMessage {
    
    /// <summary>
    /// The username of the sender.
    /// </summary>
    public string Sender { get; set; }

    /// <summary>
    /// The content of the message.
    /// </summary>
    public string Text { get; set; }


    public ChatMessage(string sender, string text) {
        Sender = sender;
        Text = text;
    }
}