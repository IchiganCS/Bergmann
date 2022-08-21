using Bergmann.Shared.Networking.Messages;

namespace Bergmann.Shared.Networking.Server;

[MessagePack.MessagePackObject]
/// <summary>
/// The name is inferred from the boxed user id.
/// </summary>
public class ChatMessageReceivedMessage : IMessage {

    /// <summary>
    /// The content of the message.
    /// </summary>
    public string Text { get; private set; }

    public string Sender { get; private set; }


    public ChatMessageReceivedMessage(string text, string sender) {
        Text = text;
        Sender = sender;
    }
}