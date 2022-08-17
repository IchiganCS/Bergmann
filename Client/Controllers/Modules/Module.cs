namespace Bergmann.Client.Controllers.Modules;

/// <summary>
/// A module is some part of a controller which can be added to better separate differnet parts of a controller. 
/// A controller might contain many, many differnet tasks, so it makes sense to split it up.
/// </summary>
public abstract class Module {

    /// <summary>
    /// Whether the module is currently active.
    /// </summary>
    public bool IsActive { get; private set; } = false;

    /// <summary>
    /// Whether the module is currently on top (the parent controller is on top).
    /// </summary>
    public bool IsOnTop { get; private set; } = false;

    /// <summary>
    /// The owner of the current module. The activate and deactivate methods are mirrored from this object.
    /// </summary>
    public Controller? Parent { get; private set; }

    /// <summary>
    /// <seealso cref="Controller.OnActivated"/>
    /// </summart>
    public virtual void OnActivated(Controller parent) {
        Parent = parent;
        IsActive = true;
    }

    /// <summary>
    /// <seealso cref="Controller.OnDeactivated"/>
    /// </summart>
    public virtual void OnDeactivated() {
        IsActive = false;
    }

    /// <summary>
    /// <seealso cref="Controller.OnNowOnTop"/>
    /// </summart>
    public virtual void OnNowOnTop() {
        IsOnTop = true;
    }

    /// <summary>
    /// <seealso cref="Controller.OnNotOnTop"/>
    /// </summart>
    public void OnNotOnTop() {
        IsOnTop = false;
    }
}