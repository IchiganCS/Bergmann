using Bergmann.Client.Graphics.Renderers;
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
    public FPHandler Fph { get; set; }

    /// <summary>
    /// The chat to be displayed when called for.
    /// </summary>
    public ChatController Chat { get; set; }


    public bool DebugViewEnabled { get; set; } = false;

    public GameController(FPHandler fph, ChatController chat) {
        Fph = fph;
        Chat = chat;

        
        Chat.Commands.Add(new() {
            Execute = args =>
                DebugViewEnabled = !DebugViewEnabled,
            Name = "debug"
        });
    }

    public override void HandleInput(UpdateArgs updateArgs) {
        Fph.HandleInput(updateArgs);

        if (updateArgs.KeyboardState.IsKeyPressed(Keys.Enter))
            ToPush = Chat;

        if (updateArgs.KeyboardState.IsKeyPressed(Keys.F1))
            DebugViewEnabled = !DebugViewEnabled;
    }
}