using Bergmann.Client.InputHandlers;
using Bergmann.Shared.Networking;
using Microsoft.AspNetCore.SignalR.Client;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bergmann.Client.Controllers;


/// <summary>
/// A controller for a chat. One may register commands and input text. The renderer for this is given by ChatRenderer.
/// It eventually renders tooltips and already sent messages too.
/// </summary>
public class ChatController : ControllerBase {
    public override CursorState RequestedCursorState => CursorState.Normal;

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
    /// If the entered text is no command, but a normal text message, this action is executed with the input string as an argument.
    /// </summary>
    public Action<string> NonCommandAction { get; set; }

    /// <summary>
    /// The input field of the chat controller.
    /// </summary>
    public TextHandler InputField { get; private set; }


    /// <summary>
    /// Constructs a new chat controller. It creates a new input field and registeres necessary events.
    /// </summary>
    /// <param name="messageAction">The action to be executed on a normal message. See <see cref="NonCommandAction"/>.</param>
    public ChatController(Action<string> messageAction) {
        NonCommandAction = messageAction;

        InputField = new();
        InputField.SpecialActions.Add((Keys.Enter, (ks) => {
            if (InputField.Text.StartsWith(CommandPrefix)) {
                string text = InputField.Text.Remove(0, CommandPrefix.Length).Trim();
                string[] elems = text.Split();
                if (elems.Length == 0)
                    return; //TODO no command given

                string command = elems[0];
                string[] args = elems.Skip(1).ToArray();

                foreach (Command cmd in Commands) {
                    if (cmd.Name.ToLower() == command.ToLower())
                        cmd.Execute?.Invoke(args);
                }

                if (!Commands.Any(x => x.Name == command))
                    return; //TODO command not found
            }
            else
                NonCommandAction(InputField.Text);

            InputField.SetText("");
            ShouldPop = true;
        }
        ));



        Hubs.Chat?.On<string, string>(Names.ReceiveMessage, (x, y) => {
            Console.WriteLine($"{x} wrote {y}");
        });

    }

    /// <summary>
    /// Forwards typing events to its input field (and special actions) and pops on escape.
    /// </summary>
    /// <param name="updateArgs">The update arguments forwarded to the input field.</param>
    public override void HandleInput(UpdateArgs updateArgs) {
        if (updateArgs.KeyboardState.IsKeyDown(Keys.Escape)) {
            InputField.SetText("");
            ShouldPop = true;
        }

        else {
            InputField.HandleInput(updateArgs);
        }
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
        public Action<string[]>? Execute { get; set; }
    }
}