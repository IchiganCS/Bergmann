using Bergmann.Client.Graphics;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers.UI;
using Bergmann.Client.InputHandlers;
using Bergmann.Shared.Networking.Client;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bergmann.Client.Controllers;


/// <summary>
/// A controller for a chat. One may register commands and input text. The renderer for this is given by ChatRenderer.
/// It eventually renders tooltips, but not sent messages. Rendering those is the task of a ChatModule. 
/// </summary>
public class ChatController : Controller {

    /// <summary>
    /// While the chat is shown, we request that the move can move freely.
    /// </summary>
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
    /// The input field of the chat controller.
    /// </summary>
    public TextHandler InputField { get; private set; }

    /// <summary>
    /// This renders the <see cref="InputField"/>
    /// </summary>
    private TextRenderer? InputRenderer { get; set; }


    /// <summary>
    /// Constructs a new chat controller. It creates a new input field and registeres necessary events.
    /// </summary>
    /// <param name="messageAction">The action to be executed on a normal message. See <see cref="NonCommandAction"/>.</param>
    public ChatController() {

        InputField = new();

        //when enter is pressed, handle either a command or send the message to the server.
        InputField.SpecialActions.Add((Keys.Enter, async (ks) => {
            if (InputField.Text.StartsWith(CommandPrefix)) {
                string text = InputField.Text.Remove(0, CommandPrefix.Length).Trim();
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
            }
            else {
                if (!string.IsNullOrWhiteSpace(InputField.Text))
                    await Connection.Active.Send(new ChatMessageSentMessage(InputField.Text));
            }

            GlThread.Invoke(() => InputField.SetText(""));
            Stack!.Pop(this);
        }
        ));
        InputHandlers.Add(InputField);
    }

    /// <summary>
    /// Forwards typing events to its input field (and special actions) and pops on escape.
    /// </summary>
    /// <param name="updateArgs">The update arguments forwarded to the input field.</param>
    public override void Update(UpdateArgs updateArgs) {
        if (updateArgs.KeyboardState.IsKeyDown(Keys.Escape)) {
            GlThread.Invoke(() => InputField.SetText(""));
            Stack!.Pop(this);
        }

        else {
            base.Update(updateArgs);
        }
    }

    /// <summary>
    /// Render the input field. Later, maybe tooltips or suggestions can be shown too.
    /// </summary>
    public override void Render(RenderUpdateArgs args) {
        Program.Active = SharedGlObjects.UIProgram;

        InputRenderer?.Render();
    }

    public override void OnActivated(ControllerStack stack) {
        base.OnActivated(stack);

        GlThread.Invoke(() => {
            InputRenderer = new() {
                PercentageAnchorOffset = (0, 0),
                AbsoluteAnchorOffset = (30, 30),
                Dimension = (-1, 70),
                RelativeAnchor = (0, 0)
            };
            InputRenderer.ConnectToTextInput(InputField);
        });

    }

    public override void OnDeactivated() {
        base.OnDeactivated();

        GlThread.Invoke(() => InputRenderer?.Dispose());
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