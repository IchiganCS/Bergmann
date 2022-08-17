using Bergmann.Client.InputHandlers;
using Bergmann.Shared;

namespace Bergmann.Client.Controllers;

/// <summary>
/// A stack of <see cref="Controller"/>s. The topmost is updated in the <see cref="Update"/> method. All are rendered.
/// </summary>
public class ControllerStack {
    /// <summary>
    /// The stack of controllers.
    /// </summary>
    private Stack<Controller> Controllers { get; set; }

    /// <summary>
    /// Contstructs a new controller stack and pushes <paramref name="root"/> to it.
    /// </summary>
    /// <param name="root">The root controller of the stack. It is never popped.</param>
    public ControllerStack(Controller root) {
        Controllers = new();
        Controllers.Push(root);
        root.OnActivated(this);
        root.OnNowOnTop();
    }


    /// <param name="args">The arguments forwarded to the top entry of the stack</param>
    public void Update(UpdateArgs args) {
        if (Controllers.Count == 0) {
            Logger.Warn("Handler stack is empty");
            return;
        }

        Controller formerTop = Controllers.Peek();
        formerTop.HandleInput(args);
    }

    public void Render() {
        foreach (var cont in Controllers)
            cont.Render();
    }

    public void Pop(Controller controller) {
        if (!Object.ReferenceEquals(Top, controller))
            return;

        controller.OnNotOnTop();
        controller.OnDeactivated();
        Controllers.Pop();

        if (Controllers.Count > 0)
            Top.OnNowOnTop();
    }

    public void Push(Controller controller) {
        if (Controllers.Count > 0)
            Top.OnNotOnTop();

        controller.OnActivated(this);
        controller.OnNowOnTop();
        Controllers.Push(controller);
    }

    /// <summary>
    /// Gets the top of all <see cref="Controller"/>s.
    /// </summary>
    public Controller Top
            => Controllers.Peek();
}