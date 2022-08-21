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

    /// <summary>
    /// Updates the top of the controller stack. This updating method is subject to change.
    /// </summary>
    /// <param name="args"></param>
    public void Update(UpdateArgs args) {
        if (Controllers.Count == 0) {
            Logger.Warn("Handler stack is empty");
            return;
        }

        Top.Update(args);

        foreach (Controller cont in Controllers)
            cont.WeakUpdate(args);
    }

    /// <summary>
    /// Traverses through the controller stack with the top item being the last to be rendered.
    /// </summary>
    /// <param name="args">The arguments being forwarded to all renderers.</param>
    public void Render(RenderUpdateArgs args) {
        foreach (var cont in Controllers.Reverse().ToArray())
            cont.Render(args);
    }

    /// <summary>
    /// Pops an item from the controller stack.
    /// </summary>
    /// <param name="controller">The item which is at the top of the stack. 
    /// This is used for checking that the wrong controller is not accidentally popped.</param>
    public void Pop(Controller controller) {
        if (!Object.ReferenceEquals(Top, controller) || Controllers.Count <= 1)
            return;

        controller.OnNotOnTop();
        controller.OnDeactivated();
        Controllers.Pop();

        if (Controllers.Count > 0)
            Top.OnNowOnTop();
    }

    /// <summary>
    /// Pushes a new controller to the stack. Activation methods are called accordingly and automatically.
    /// </summary>
    /// <param name="controller">The new controller which is soon on top.</param>
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