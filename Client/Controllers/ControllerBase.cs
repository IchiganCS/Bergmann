using Bergmann.Client.InputHandlers;
using OpenTK.Windowing.Common;

namespace Bergmann.Client.Controllers;

/// <summary>
/// A compelete handler for the current state. It can stand on its own and handels input for itself.
/// The game holds a stack of these and calls the methods inherited from
/// <see cref="IInputHandler"/> on this object. It can specify if a <see cref="ControllerBase"/> should be pushed
/// or this should be popped. Of course it may hold other <see cref="IInputHandler"/> as some kind of children and forward
/// the method calls.
/// </summary>
public abstract class ControllerBase : IInputHandler {
    
    /// <summary>
    /// Whether a new <see cref="ControllerBase"/> should be pushed. Is to be checked after every update.
    /// Can be null if none is requested.
    /// </summary>
    public ControllerBase? ToPush { get; set; } = null;

    /// <summary>
    /// Is true if this should be popped from the stack of all <see cref="ControllerBase"/>s.
    /// False if no action is required.
    /// </summary>
    public bool ShouldPop { get; set; } = false;

    /// <summary>
    /// Specifies whether the given controller is in any place in the <see cref="ControllerStack"/>.
    /// </summary>
    /// <value></value>
    public bool IsActive { get; set; } = false;

    /// <summary>
    /// Specifies whether the given controller is on top of the <see cref="ControllerStack"/>.
    /// </summary>
    public bool IsOnTop { get; set; } = false;

    /// <summary>
    /// Window is responsible to make this property the current cursor state if this is on top of the stack.
    /// Of course, when tabbing and so on, this is not enforced. For normal handling though, this is the current state.
    /// </summary>
    public abstract CursorState RequestedCursorState { get; }


    public abstract void HandleInput(UpdateArgs updateArgs);
}