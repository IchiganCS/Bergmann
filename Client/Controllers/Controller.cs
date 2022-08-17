using Bergmann.Client.InputHandlers;
using Bergmann.Client.Controllers.Modules;
using OpenTK.Windowing.Common;
using Bergmann.Client.Graphics.Renderers;

namespace Bergmann.Client.Controllers;

/// <summary>
/// A controller marks a new part of controlling the game: For example, when the chat is opened, all other input of the game is 
/// stopped and some systems may be disabled. Or when the player opens their inventory.
/// </summary>
public abstract class Controller {

    protected ControllerStack? Stack { get; private set; }

    protected IList<Module> Modules { get; set; } = new List<Module>();
    protected IList<IInputHandler> InputHandlers { get; set; } = new List<IInputHandler>();
    protected IList<IRenderer> Renderers { get; set; } = new List<IRenderer>();

    public bool IsOnTop { get; private set; }

    /// <summary>
    /// Window is responsible to make this property the current cursor state if this is on top of the stack.
    /// Of course, when tabbing and so on, this is not enforced. For normal handling though, this is the current state.
    /// </summary>
    public abstract CursorState RequestedCursorState { get; }


    public virtual void HandleInput(UpdateArgs updateArgs) {
        foreach (var input in InputHandlers)
            input.HandleInput(updateArgs);
    }

    public virtual void Render() {
        foreach (var renderer in Renderers)
            renderer.Render();
    }


    public virtual void OnActivated(ControllerStack stack) {
        Stack = stack;
        foreach (var child in Modules)
            child.OnActivated(this);
    }

    public virtual void OnDeactivated() {
        Stack = null;
        foreach (var child in Modules)
            child.OnDeactivated();
    }
    public virtual void OnNowOnTop() {
        IsOnTop = true;
        foreach (var child in Modules)
            child.OnNowOnTop();
    }
    public virtual void OnNotOnTop() {
        IsOnTop = false;
        foreach (var child in Modules)
            child.OnNotOnTop();
    }
}