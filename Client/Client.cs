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

        nwSet.APIVersion = Version.Parse("3.3");

        

        using Window win = new(gwSet, nwSet);
        win.Run();
    }
}