using Bergmann.Client.Controllers;
using Bergmann.Client.InputHandlers;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.Renderers;


public class ChatRenderer : IUIRenderer {
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

    public bool PointInShape(Vector2 point, Vector2 size)
        => InputRenderer.PointInShape(point, size);

    public void Render() {
        if (Chat.IsActive)
            InputRenderer.Render();
    }


    public void Dispose() {

    }
}