using Bergmann.Client.Graphics;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers.UI;
using Bergmann.Client.InputHandlers;
using Bergmann.Shared.Networking.Client;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bergmann.Client.Controllers.Modules;

/// <summary>
/// A module to handle commands and chat messages. It can also render itself and show tooltips if necessary.
/// </summary>
public class ChatWriteModule : Module {


    /// <summary>
    /// The prefix for every command. It is recommended not to change this.
    /// </summary>
    public const string CommandPrefix = "/";

    /// <summary>
    /// A list of all commands exectuable by the chat. If a message with <see cref="CommandPrefix"/> is sent,
    /// a fitting command is searched for and executed.
    /// </summary>
    public IList<Command> Commands { get; set; } = new List<Command>();

    /// <summary>
    /// The action to be executed when the message didn't start with the command prefix.
    /// </summary>
    public Action<string> NonCommandAction { get; set; }


    public ChatWriteModule(Action<string> nonCommand) {
        NonCommandAction = nonCommand;

        Input = new();
        Input.SpecialActions.Add((Keys.Enter, ks => {
            if (Input.Text.StartsWith(CommandPrefix)) {
                string text = Input.Text.Remove(0, CommandPrefix.Length).Trim();
                string[] elems = text.Split();
                if (elems.Length > 0) {
                    string command = elems[0];
                    string[] args = elems.Skip(1).ToArray();

                    foreach (Command cmd in Commands) {
                        if (cmd.Name.ToLower() == command.ToLower())
                            cmd.Execute?.Invoke(args);
                    }

                    if (!Commands.Any(x => x.Name == command)) {
                        //TODO command not found
                    }
                }
                else {
                    //TODO no command given
                }
            }
            else {
                NonCommandAction(Input.Text);
            }

            Input.SetText("");
        }
        ));
    }

    /// <summary>
    /// The input field. It is connected to the renderer.
    /// </summary>
    private TextHandler Input { get; set; }

    /// <summary>
    /// The renderer for the <see cref="Input"/>.
    /// </summary>
    private TextRenderer? Renderer { get; set; }


    public override void OnActivated(Controller parent) {
        base.OnActivated(parent);

        GlThread.Invoke(() => {
            Renderer = new() {
                Dimension = (-1, 70),
                AbsoluteAnchorOffset = (50, 50),
                PercentageAnchorOffset = (0, 0),
                RelativeAnchor = (0, 0)
            };
            Renderer.ConnectToTextInput(Input);
        });
    }

    public override void OnDeactivated() {
        base.OnDeactivated();

        GlThread.Invoke(() => {
            Renderer?.Dispose();
            Renderer = null;
        });
    }

    public void Render() {
        Program.Active = GlObjects.UIProgram;

        Renderer?.Render();
    }

    public void Update(UpdateArgs args) {
        Input.HandleInput(args);
    }




    /// <summary>
    /// A command executable by a chat message.
    /// </summary>
    public class Command {
        /// <summary>
        /// The name is also the identifier which the user has to type.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Arguments for a command are specified through spaces between words.
        /// </summary>
        public Action<string[]> Execute { get; set; } = null!;
    }
}