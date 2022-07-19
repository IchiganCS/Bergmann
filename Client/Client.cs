using Bergmann.Client.Graphics;
using Bergmann.Shared;
using OpenTK.Windowing.Desktop;

namespace Bergmann.Client;

public class Client {
    public static void Main(string[] args) {
        Logger.Info("Starting application");
        GameWindowSettings gwSet = GameWindowSettings.Default;
        NativeWindowSettings nwSet = NativeWindowSettings.Default;

        gwSet.RenderFrequency = 60f;
        gwSet.UpdateFrequency = 140f;
        nwSet.Title = "Bergmann";
        nwSet.Size = (1600, 1200);
        nwSet.CurrentMonitor = Monitors.GetPrimaryMonitor().Handle;
        nwSet.Location = (0, 0);

        nwSet.APIVersion = Version.Parse("3.3");

        

        using Window win = new(gwSet, nwSet);
        win.Run();
    }
}