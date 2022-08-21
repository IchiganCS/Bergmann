using Bergmann.Shared.Networking.Messages;

namespace Bergmann.Shared.Networking.Client;

[MessagePack.MessagePackObject]
/// <summary>
/// The name is inferred from the boxed user id.
/// </summary>
public class ChatMessageSentMessage : IMessage {

    /// <summary>
    /// The content of the message.
    /// </summary>
    public string Text { get; set; }


    public ChatMessageSentMessage(string text) {
        Text = text;
    }
}