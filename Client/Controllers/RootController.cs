using Bergmann.Client.InputHandlers;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = Bergmann.Client.Graphics.Window;

namespace Bergmann.Client.Controllers;


public class RootController : ControllerBase {

    public FPHandler FPH { get; set; }
    public ChatController Chat { get; set; }

    public RootController(FPHandler fph, ChatController chat) {
        FPH = fph;
        Chat = chat;
    }

    public override void HandleInput(UpdateArgs updateArgs) {
        FPH.HandleInput(updateArgs);

        if (updateArgs.KeyboardState.IsKeyPressed(Keys.Enter))
            ToPush = Chat;

        if (updateArgs.KeyboardState.IsKeyPressed(Keys.Escape))
            Window.Instance.CursorState = CursorState.Normal;
    }
}