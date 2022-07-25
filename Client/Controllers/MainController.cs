using Bergmann.Client.InputHandlers;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bergmann.Client.Controllers;

/// <summary>
/// Handles the base inputs of the game while the player is moving around etc.
/// </summary>
public class GameController : ControllerBase {
    
    /// <summary>
    /// Grab the cursor, we only want our cross drawn.
    /// </summary>
    public override CursorState RequestedCursorState => CursorState.Grabbed;

    /// <summary>
    /// The first person handler of the player. It is to be updated when the root game controller is active.
    /// </summary>
    public FPHandler FPH { get; set; }

    /// <summary>
    /// The chat to be displayed when called for.
    /// </summary>
    public ChatController Chat { get; set; }

    public GameController(FPHandler fph, ChatController chat) {
        FPH = fph;
        Chat = chat;
    }

    public override void HandleInput(UpdateArgs updateArgs) {
        FPH.HandleInput(updateArgs);

        if (updateArgs.KeyboardState.IsKeyPressed(Keys.Enter))
            ToPush = Chat;
    }
}