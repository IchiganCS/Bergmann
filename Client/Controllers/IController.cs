namespace Bergmann.Client.Controllers;

public interface IController {
    public void OnActivated(ControllerStack stack);
    public void OnDeactivated();

    public void OnNowOnTop();
    public void OnNotOnTop();
}