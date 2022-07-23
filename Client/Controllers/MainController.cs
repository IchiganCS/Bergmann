using Bergmann.Client.InputHandlers;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Window = Bergmann.Client.Graphics.Window;

namespace Bergmann.Client.Controllers;


public class MainController : ControllerBase {
    public override CursorState RequestedCursorState => CursorState.Grabbed;

    public FPHandler FPH { get; set; }
    public ChatController Chat { get; set; }

    public MainController(FPHandler fph, ChatController chat) {
        FPH = fph;
        Chat = chat;
    }

    public override void HandleInput(UpdateArgs updateArgs) {
        FPH.HandleInput(updateArgs);

        if (updateArgs.KeyboardState.IsKeyPressed(Keys.Enter))
            ToPush = Chat;
    }
}