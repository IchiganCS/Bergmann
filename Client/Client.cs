using Bergmann.Client.Graphics;
using Bergmann.Shared;
using OpenTK.Windowing.Desktop;

namespace Bergmann.Client;

public class Client {
    public static void Main(string[] args) {
        Logger.Info("Starting client...");

        GameWindowSettings gwSet = GameWindowSettings.Default;
        NativeWindowSettings nwSet = NativeWindowSettings.Default;

        gwSet.RenderFrequency = 00f;
        gwSet.UpdateFrequency = 140f;

        nwSet.APIVersion = new(3, 3);
        nwSet.Title = "Bergmann";
        nwSet.Size = (1400, 1100);
        nwSet.Location = (100, 100);

        

        using Window win = new(gwSet, nwSet);
        Window.Instance = win;
        win.Run();

        Logger.Info("Exiting client");
    }

    public static void ConnectTo(string link) {

    }
}