using Bergmann.Client.Controllers.Modules;
using Bergmann.Client.Graphics;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers;
using Bergmann.Client.InputHandlers;
using OpenTK.Windowing.Common;

namespace Bergmann.Client.Controllers;

/// <summary>
/// Stores a few modules which should run the entire time. This should be the root of the controller stack.
/// </summary>
public class ServiceController : Controller {
    public override CursorState RequestedCursorState => CursorState.Normal;
    ChatModule Chat;

    public ServiceController() {
        Chat = new();

        Modules.Add(Chat);
    }

    public override void Render(RenderUpdateArgs args) {
        Program.Active = SharedGlObjects.UIProgram;

        Chat.Render();
    }

    public override void HandleInput(InputUpdateArgs updateArgs) {
        base.HandleInput(updateArgs);
        
        Stack!.Push(new GameController());
    }
}