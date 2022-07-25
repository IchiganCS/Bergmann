using System.Diagnostics;
using Bergmann.Client.Graphics;
using Bergmann.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using OpenTK.Windowing.Desktop;

namespace Bergmann.Client;

public class Client {
    public static string ServerAddress = "http://localhost:5000/";

    public static void Main(string[] args) {
        
        Logger.Info("Starting client...");

        ProcessStartInfo psi = new() {
            CreateNoWindow = true,
            FileName = "Server/bin/Debug/net6.0/Server",
        };

        Process server = Process.Start(psi)!;

        Thread.Sleep(300);




        GameWindowSettings gwSet = GameWindowSettings.Default;
        NativeWindowSettings nwSet = NativeWindowSettings.Default;

        gwSet.RenderFrequency = 60f;
        gwSet.UpdateFrequency = 140f;
        nwSet.Title = "Bergmann";
        nwSet.Size = (1400, 1100);
        nwSet.CurrentMonitor = Monitors.GetPrimaryMonitor().Handle;
        nwSet.Location = (0, 0);

        nwSet.APIVersion = Version.Parse("3.3");



        using Window win = new(gwSet, nwSet);
        Window.Instance = win;
        win.Run();

        server.Kill();
        Logger.Info("Exiting client");
    }
}