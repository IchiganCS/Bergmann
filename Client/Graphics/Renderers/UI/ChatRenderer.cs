using Bergmann.Client.Controllers;

namespace Bergmann.Client.Graphics.Renderers.UI;


public class ChatRenderer : UIRenderer {
    public ChatController Chat { get; private set; }
    private TextRenderer InputRenderer { get; set; }

    public ChatRenderer(ChatController chat) {
        Chat = chat;
        InputRenderer = new() {
            AbsoluteAnchorOffset = (30, 30),
            PercentageAnchorOffset = (0, 0),
            RelativeAnchor = (0, 0),
            Dimension = (-1, 70)
        };
        InputRenderer.HookTextField(chat.InputField);
    }


    public override void Render() {
        if (Chat.IsActive)
            InputRenderer.Render();
    }


    public override void Dispose() {

    }
}