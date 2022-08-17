namespace Bergmann.Client.Controllers.Modules;

/// <summary>
/// A module is some part of a controller which can be added to better separate differnet parts of a controller. 
/// A controller might contain many, many differnet tasks, so it makes sense to split it up.
/// </summary>
public abstract class Module {

    public bool IsActive { get; private set; }

    public bool IsOnTop { get; private set; }

    /// <summary>
    /// The owner of the current module.
    /// </summary>
    public Controller? Parent { get; private set; }

    public virtual void OnActivated(Controller parent) {
        Parent = parent;
        IsActive = false;
    }
    public virtual void OnDeactivated() {
        IsActive = false;
    }

    public virtual void OnNowOnTop() {
        IsOnTop = true;
    }
    public void OnNotOnTop() {
        IsOnTop = false;
    }
}