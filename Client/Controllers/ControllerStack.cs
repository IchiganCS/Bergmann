using Bergmann.Client.InputHandlers;
using Bergmann.Shared;

namespace Bergmann.Client.Controllers;

/// <summary>
/// A stack of <see cref="ParentController"/>s. The topmost is updated in the <see cref="Execute"/> method. All are rendered.
/// </summary>
public class ControllerStack {
    /// <summary>
    /// The stack of controllers.
    /// </summary>
    private Stack<ParentController> Controllers { get; set; }

    /// <summary>
    /// Contstructs a new controller stack and pushes <paramref name="root"/> to it.
    /// </summary>
    /// <param name="root">The root controller of the stack. It is never popped.</param>
    public ControllerStack(ParentController root) {
        Controllers = new();
        Controllers.Push(root);
        root.OnActivated(this);
        root.OnNowOnTop();
    }

    /// <summary>
    /// Executes an update cycle: Calls update on the top entry of the stack and performs additional tasks
    /// depending on whether the controller wrote to its <see cref="ParentController.ShouldPop"/> or 
    /// <see cref="ParentController.ToPush"/> values. If so, those are executed accordingly.
    /// </summary>
    /// <param name="args">The arguments forwarded to the top entry of the stack</param>
    public void Execute(UpdateArgs args) {
        if (Controllers.Count == 0) {
            Logger.Warn("Handler stack is empty");
            return;
        }

        ParentController formerTop = Controllers.Peek();
        formerTop.HandleInput(args);

        if (formerTop.ShouldPop) {
            formerTop.ShouldPop = false;
            if (Controllers.Count == 1) {
                Logger.Warn("Received command to pop root of the controller stack. Aborted");
                return;
            }

            formerTop.OnNotOnTop();
            formerTop.OnDeactivated();
            Controllers.Pop();
        }
        if (formerTop.ToPush is not null) {
            Controllers.Push(formerTop.ToPush);
            formerTop.ToPush = null;
            Top.OnActivated(this);
        }

        if (Top != formerTop)
            Top.OnNowOnTop();
    }

    /// <summary>
    /// Gets the top of all <see cref="ParentController"/>s.
    /// </summary>
    public ParentController Top
        => Controllers.Peek();
}