using Bergmann.Client.Controllers.Modules;
using Bergmann.Client.Graphics;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers.UI;
using Bergmann.Shared.Networking.Client;
using OpenTK.Windowing.Common;

namespace Bergmann.Client.Controllers;

/// <summary>
/// Stores a few modules which should run the entire time. This should be the root of the controller stack accordingly.
/// Examples are listeners for chat messages
/// </summary>
public class ServiceController : Controller {
    public override CursorState RequestedCursorState => CursorState.Normal;

    private ChatWriteModule ChatWriter { get; init; }

    private TextRenderer? ServerTest { get; init; }

    public ServiceController() {
        ChatWriter = new(msg => { });

        ChatWriter.Commands.Add(new() {
            Name = "login",
            Execute = async args => {
                if (args.Length < 2 || Connection.Active is null)
                    return;

                await Connection.Active.SendAsync(new LogInAttemptMessage(args[0], args[1]));
            },
            Description = "Logs you in with a specified username and password. May only be executed when there is a connection",
            Arguments = new() {
                ("name", "The username with which you want to login"),
                ("password", "The password to authenticate you with")
            }
        });
        ChatWriter.Commands.Add(new() {
            Name = "connect",
            Execute = args => {
                if (args.Length < 1)
                    return;

                Connection.Active = new(args[0]);
            },
            Description = "Connects you to a server.",
            Arguments = new() {
                ("address", "The address of the server (the port is optional, defaults to 23156)")
            }
        });

        ServerTest = new() {
            Dimension = (-1, 80),
            AbsoluteAnchorOffset = (0, 0),
            PercentageAnchorOffset = (0.5f, 0.7f),
            RelativeAnchor = (0.5f, 0.5f)
        };
        ServerTest.SetText("Not connected");

        Modules.Add(ChatWriter);
    }

    public override void Update(UpdateArgs updateArgs) {
        base.Update(updateArgs);

        if (Connection.Active is null || Connection.Active.UserID is null)
            ChatWriter.Update(updateArgs);
        else
            Stack!.Push(new GameController());
    }

    public override void Render(RenderUpdateArgs args) {
        base.Render(args);

        if (IsOnTop) {
            Program.Active = GlObjects.UIProgram; 
            Program.Active.SetUniform("windowsize", Window.Instance.Size);
            if (Connection.Active is not null) {
                ServerTest?.SetText($"Connected to {Connection.Active.Link} - not logged in.");
            }
            ServerTest?.Render();
            ChatWriter.Render();
        }
    }
}