using Bergmann.Client.InputHandlers;
using OpenTK.Windowing.GraphicsLibraryFramework;

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
    }
}