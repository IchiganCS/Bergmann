using Bergmann.Client.InputHandlers;
using Bergmann.Shared;

namespace Bergmann.Client.Controllers;

/// <summary>
/// A stack of <see cref="ControllerBase"/>s. The topmost is updated in the <see cref="Execute"/> method. All are rendered.
/// </summary>
public class ControllerStack {
    /// <summary>
    /// The stack of controllers.
    /// </summary>
    private Stack<ControllerBase> Controllers { get; set; }

    /// <summary>
    /// Contstructs a new controller stack and pushes <paramref name="root"/> to it.
    /// </summary>
    /// <param name="root">The root controller of the stack. It is never popped.</param>
    public ControllerStack(ControllerBase root) {
        Controllers = new();
        Controllers.Push(root);
        root.IsActive = true;
        root.IsOnTop = true;
    }

    /// <summary>
    /// Executes an update cycle: Calls update on the top entry of the stack and performs additional tasks
    /// depending on whether the controller wrote to its <see cref="ControllerBase.ShouldPop"/> or 
    /// <see cref="ControllerBase.ToPush"/> values. If so, those are executed accordingly.
    /// </summary>
    /// <param name="args">The arguments forwarded to the top entry of the stack</param>
    public void Execute(UpdateArgs args) {
        if (Controllers.Count == 0) {
            Logger.Warn("Handler stack is empty");
            return;
        }

        ControllerBase top = Controllers.Peek();
        top.HandleInput(args);
        top.IsOnTop = false;

        if (top.ShouldPop) {
            top.ShouldPop = false;
            if (Controllers.Count == 1) {
                Logger.Warn("Received command to pop root of the controller stack. Aborted");
                return;
            }

            top.IsActive = false;
            Controllers.Pop();
        }
        if (top.ToPush is not null) {
            Controllers.Push(top.ToPush);
            top.ToPush = null;
        }

        Controllers.Peek().IsOnTop = true;
        Controllers.Peek().IsActive = true;
    }
}