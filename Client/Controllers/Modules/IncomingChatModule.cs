using Bergmann.Client.Graphics;
using Bergmann.Client.Graphics.Renderers.UI;
using Bergmann.Shared.Networking;
using Bergmann.Shared.Networking.Messages;
using Bergmann.Shared.Networking.Server;

namespace Bergmann.Client.Controllers.Modules;

/// <summary>
/// A module which handles different tasks for incoming chat messages. It receives new chat messages, holds the history
/// and renders them when requested. This module might be removed and merged with <see cref="ChatController"/>.
/// </summary>
public class IncomingChatModule : Module, IMessageHandler<ChatMessageReceivedMessage> {

    /// <summary>
    /// The history of all messages received.
    /// </summary>
    private List<ChatMessageReceivedMessage> History { get; set; } = new();

    private TextRenderer? TopMessage { get; set; }


    public void HandleMessage(ChatMessageReceivedMessage message) {
        History.Add(message);


    }

    public override void OnActivated(Controller parent) {
        base.OnActivated(parent);

        GlThread.Invoke(() => TopMessage = new() {
            AbsoluteAnchorOffset = (30, 150),
            PercentageAnchorOffset = (0, 0),
            Dimension = (-1, 70),
            RelativeAnchor = (0, 0)
        });

        Connection.Active.RegisterMessageHandler(this);
    }

    public override void OnDeactivated() {
        base.OnDeactivated();

        GlThread.Invoke(() => TopMessage?.Dispose());

        Connection.Active.DropMessageHandler(this);
    }

    public void Render() {
        if (History.Count == 0)
            TopMessage?.SetText("");
        else
            TopMessage?.SetText(History.FindLast(x => true)!.Text);

        TopMessage?.Render();
    }
}