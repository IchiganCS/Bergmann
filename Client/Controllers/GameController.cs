using Bergmann.Client.InputHandlers;
using Bergmann.Client.Controllers.Modules;
using Bergmann.Shared.Networking;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bergmann.Client.Controllers;

/// <summary>
/// Handles the base inputs of the game while the player is moving around etc.
/// </summary>
public class GameController : Controller {

    /// <summary>
    /// Grab the cursor, we only want our cross drawn.
    /// </summary>
    public override CursorState RequestedCursorState => CursorState.Grabbed;

    /// <summary>
    /// The first person handler of the player. It is to be updated when the root game controller is active.
    /// </summary>
    public FPHandler Fph { get; init; }

    /// <summary>
    /// The chat to be displayed when called for.
    /// </summary>
    public ChatController Chat { get;  init; }



    public GameController() {
        Chat = new(async x => {
            if (string.IsNullOrWhiteSpace(x))
                return;
                
            await Connection.Active!.ClientToServerAsync(new ChatMessage("ich", x));
        });
        Fph = new();

        Modules.Add(new WorldLoaderModule(() => Fph.Position));
        InputHandlers.Add(Fph);
    }

    public override void HandleInput(UpdateArgs updateArgs) {
        base.HandleInput(updateArgs);

        if (updateArgs.KeyboardState.IsKeyPressed(Keys.Enter))
            Stack!.Push(Chat);
    }
}