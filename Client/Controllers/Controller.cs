using Bergmann.Client.InputHandlers;
using Bergmann.Client.Controllers.Modules;
using OpenTK.Windowing.Common;

namespace Bergmann.Client.Controllers;

/// <summary>
/// A controller marks a new part of controlling the game: For example, when the chat is opened, all other input of the game is 
/// stopped and some systems may be disabled. Or when the player opens their inventory.
/// </summary>
public abstract class Controller {

    /// <summary>
    /// The stack which holds this controller. One may modify it to push new controllers or pop the current one.
    /// </summary>
    protected ControllerStack? Stack { get; private set; }

    /// <summary>
    /// The modules owned by this controller. A module may do anything it wants, it takes its activation state from its
    /// parent, namely <see cref="this"/>.
    /// </summary>
    /// <typeparam name="Module">Any type of module.</typeparam>
    protected IList<Module> Modules { get; set; } = new List<Module>();

    /// <summary>
    /// A list of registered input handlers. A subclass is not forced to use it, it is a commodity.
    /// When <see cref="this"/> is updated, the base implementation calls <see cref="IInputHandler.HandleInput"/> on them
    /// with the same update arguments.
    /// </summary>
    /// <typeparam name="IInputHandler">Any type of input handler.</typeparam>
    protected IList<IInputHandler> InputHandlers { get; set; } = new List<IInputHandler>();

    /// <summary>
    /// Whether the current controller is on top. This is set automatically by the activation methods.
    /// </summary>
    public bool IsOnTop { get; private set; }

    /// <summary>
    /// Window is responsible to make this property the current cursor state if this is on top of the stack.
    /// Of course, when tabbing and so on, this is not enforced. For normal handling in-game though, this is the current state.
    /// </summary>
    public abstract CursorState RequestedCursorState { get; }


    /// <summary>
    /// The controller receives an update and should be updated. Registered <see cref="InputHandlers"/> are updated automatically.
    /// </summary>
    /// <param name="updateArgs">The arguemnts which should be used for updating.</param>
    public virtual void Update(UpdateArgs updateArgs) {
        foreach (var input in InputHandlers)
            input.HandleInput(updateArgs);
    }

    /// <summary>
    /// The controller receives a weak update, that means, the input is not directed for this specific controller.
    /// Some controllers however, may decline this restriction or perform only some background updates. The
    /// default implementation is empty.
    /// </summary>
    /// <param name="update">The update args. Those are not directed for this controller.</param>
    public virtual void WeakUpdate(UpdateArgs update) {

    }

    /// <summary>
    /// Renders the controller. The default implementation is empty.
    /// </summary>
    /// <param name="args">Arguments which might be used for rendering stuff.</param>
    public virtual void Render(RenderUpdateArgs args) { }


    /// <summary>
    /// Is called when the controller enters the given stack. It is now active and should prepare to be used for rendering, maybe
    /// for input handling too. This is a good place to initialize members.
    /// </summary>
    /// <param name="stack">The stack which called the method. It is now the parent of the controller and stored in
    /// <see cref="Stack"/>.</param>
    public virtual void OnActivated(ControllerStack stack) {
        Stack = stack;
        foreach (var child in Modules)
            child.OnActivated(this);
    }

    /// <summary>
    /// The controller is deactivated. Resources should be freed here. The controller might be garbage collected at any time now.
    /// </summary>
    public virtual void OnDeactivated() {
        Stack = null;
        foreach (var child in Modules)
            child.OnDeactivated();
    }

    /// <summary>
    /// The controller is now on top of the stack and might perform special actions. It also is updated now.
    /// </summary>
    public virtual void OnNowOnTop() {
        IsOnTop = true;
        foreach (var child in Modules)
            child.OnNowOnTop();
    }

    /// <summary>
    /// The controller is no longer on top of the stack. A controller might adapt its members here.
    /// </summary>
    public virtual void OnNotOnTop() {
        IsOnTop = false;
        foreach (var child in Modules)
            child.OnNotOnTop();
    }
}