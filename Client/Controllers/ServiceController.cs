using Bergmann.Client.InputHandlers;
using OpenTK.Windowing.Common;

namespace Bergmann.Client.Controllers;

/// <summary>
/// Stores a few modules which should run the entire time. This should be the root of the controller stack.
/// </summary>
public class ServiceController : Controller {
    public override CursorState RequestedCursorState => CursorState.Normal;

    public override void HandleInput(UpdateArgs updateArgs) {
        base.HandleInput(updateArgs);

        Stack!.Push(new GameController());
    }
}