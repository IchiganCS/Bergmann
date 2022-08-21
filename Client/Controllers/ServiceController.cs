using Bergmann.Shared.Networking.Client;
using Bergmann.Shared.Networking.Messages;
using OpenTK.Windowing.Common;

namespace Bergmann.Client.Controllers;

/// <summary>
/// Stores a few modules which should run the entire time. This should be the root of the controller stack accordingly.
/// Examples are listeners for chat messages
/// </summary>
public class ServiceController : Controller {
    public override CursorState RequestedCursorState => CursorState.Normal;
    private DateTime LastLoginAttempt { get; set; } = DateTime.MinValue;

    public override async void Update(UpdateArgs updateArgs) {
        base.Update(updateArgs);
        
        if (Connection.Active.UserID is not null)
            Stack!.Push(new GameController());
        else if (DateTime.Now - LastLoginAttempt > TimeSpan.FromMilliseconds(500)) {
            LastLoginAttempt = DateTime.Now;
            await Connection.Active.Send(new LogInAttemptMessage("test", "test"));
        }
    }
}