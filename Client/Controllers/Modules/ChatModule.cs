using Bergmann.Client.Graphics;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers;
using Bergmann.Client.Graphics.Renderers.UI;
using Bergmann.Shared.Networking;

namespace Bergmann.Client.Controllers.Modules;

public class ChatModule : Module, IRenderer, IMessageHandler<ChatMessage> {
    private List<ChatMessage> Messages { get; set; } = new();

    private TextRenderer? TopMessage { get; set; }


    public void HandleMessage(ChatMessage message) {
        Messages.Add(message);


    }

    public override void OnActivated(Controller parent) {
        base.OnActivated(parent);

        GlThread.Invoke(() => TopMessage = new() {
            AbsoluteAnchorOffset = (30, 150),
            PercentageAnchorOffset = (0, 0),
            Dimension = (-1, 70),
            RelativeAnchor = (0, 0)
        });

        Connection.Active?.RegisterMessageHandler(this);
    }

    public override void OnDeactivated() {
        base.OnDeactivated();

        GlThread.Invoke(() => TopMessage?.Dispose());

        Connection.Active?.DropMessageHandler(this);
    }

    public void Render() {
        Program.Active = SharedGlObjects.UIProgram;

        if (Messages.Count == 0)
            GlThread.Invoke(() => TopMessage?.SetText(""));
        else
            GlThread.Invoke(() => TopMessage?.SetText(Messages.FindLast(x => true)!.Text));
    }

    public void Dispose() {
    }
}