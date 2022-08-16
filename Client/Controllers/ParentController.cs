using Bergmann.Client.InputHandlers;
using OpenTK.Windowing.Common;

namespace Bergmann.Client.Controllers;

/// <summary>
/// A compelete handler for the current state. It can stand on its own and handels input for itself.
/// The game holds a stack of these and calls the methods inherited from
/// <see cref="IInputHandler"/> on this object. It can specify if a <see cref="ParentController"/> should be pushed
/// or this should be popped. Of course it may hold other <see cref="IInputHandler"/> as some kind of children and forward
/// the method calls.
/// </summary>
public abstract class ParentController : IController {

    /// <summary>
    /// Whether a new <see cref="ParentController"/> should be pushed. Is to be checked after every update.
    /// Can be null if none is requested.
    /// </summary>
    public ParentController? ToPush { get; set; } = null;

    /// <summary>
    /// Is true if this should be popped from the stack of all <see cref="ParentController"/>s.
    /// False if no action is required.
    /// </summary>
    public bool ShouldPop { get; set; } = false;

    protected IList<IController> ChildControllers { get; set; } = new List<IController>();
    protected IList<IInputHandler> ChildInputHandlers { get; set; } = new List<IInputHandler>();

    public bool IsOnTop { get; private set; }

    /// <summary>
    /// Window is responsible to make this property the current cursor state if this is on top of the stack.
    /// Of course, when tabbing and so on, this is not enforced. For normal handling though, this is the current state.
    /// </summary>
    public abstract CursorState RequestedCursorState { get; }


    public virtual void HandleInput(UpdateArgs updateArgs) {
        foreach (var input in ChildInputHandlers)
            input.HandleInput(updateArgs);
    }


    public virtual void OnActivated(ControllerStack stack) {
        foreach (var child in ChildControllers)
            child.OnActivated(stack);
    }

    public virtual void OnDeactivated() {
        foreach (var child in ChildControllers)
            child.OnDeactivated();
    }
    public virtual void OnNowOnTop() {
        IsOnTop = true;
        foreach (var child in ChildControllers)
            child.OnNowOnTop();
    }
    public virtual void OnNotOnTop() {
        IsOnTop = false;
        foreach (var child in ChildControllers)
            child.OnNotOnTop();
    }
}