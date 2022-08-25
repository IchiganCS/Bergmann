using Bergmann.Client.Graphics;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers.UI;
using Bergmann.Client.InputHandlers;
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

    /// <summary>
    /// Renderers for displaying some kind of help or other commands.
    /// </summary>
    private TextRenderer?[] HelpRenderers { get; set; }

    /// <summary>
    /// The input field. It is connected to the renderer.
    /// </summary>
    private TextHandler Input { get; set; }

    /// <summary>
    /// The renderer for the <see cref="Input"/>.
    /// </summary>
    private TextRenderer? Renderer { get; set; }


    /// <summary>
    /// Constructs a new chat writer module.
    /// </summary>
    /// <param name="nonCommand">The command to execute when the entered messsage is not a command as specified in <see cref="Commands"/>.</param>
    /// <param name="helpLineCount">How many lines shown as help will be displayed. This should be 2 or more.</param>
    public ChatWriteModule(Action<string> nonCommand, int helpLineCount = 5) {
        NonCommandAction = nonCommand;
        HelpRenderers = new TextRenderer[helpLineCount];

        Input = new();
        Input.SpecialActions.Add((Keys.Enter, ks => {
            if (Input.Text.StartsWith(CommandPrefix)) {
                foreach (Command cmd in GetMatchingCommands(Input.Text)) {
                    cmd.Execute(ExtractArgs(Input.Text));
                }
                //TODO command not found, no command given
            }

            else
                NonCommandAction(Input.Text);

            Input.SetText("");
        }
        ));
    }


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

            for (int i = 0; i < HelpRenderers.Length; i++) {
                HelpRenderers[i] = new() {
                    Dimension = (-1, 70),
                    AbsoluteAnchorOffset = (50, 120 + i * 80),
                    PercentageAnchorOffset = (0, 0),
                    RelativeAnchor = (0, 0)
                };
                HelpRenderers[i]?.SetText("");
            }
        });
    }

    public override void OnDeactivated() {
        base.OnDeactivated();

        GlThread.Invoke(() => {
            Renderer?.Dispose();
            Renderer = null;
            foreach (TextRenderer? ren in HelpRenderers)
                ren?.Dispose();
        });
    }

    public void Render() {
        Program.Active = GlObjects.UIProgram;
        Renderer?.Render();
        foreach (TextRenderer? helper in HelpRenderers)
            helper?.Render();
    }

    public void Update(UpdateArgs args) {
        Input.HandleInput(args);

        // show information for available commands to help the user.
        if (Input.Text.StartsWith(CommandPrefix)) {
            Command[] matches = GetMatchingCommandsFromName(Input.Text).ToArray();
            int displayCount = Math.Min(HelpRenderers.Length, matches.Length);

            if (displayCount == 0) {
                displayCount++;
                HelpRenderers[0]?.SetText("No command found");
            }
            else if (displayCount == 1) {
                // display very detailed information about the fitting command
                int currentArgIndex = ExtractArgs(Input.Text).Length - 1;

                if (Input.Text.EndsWith(" "))
                    currentArgIndex++;

                if (matches[0].Arguments.Count != 0 && currentArgIndex >= 0) {
                    var argList = matches[0].Arguments;

                    if (currentArgIndex >= argList.Count)
                        currentArgIndex = argList.Count - 1;

                    var argName = argList[currentArgIndex].Item1;
                    var argDesc = argList[currentArgIndex].Item2;
                    HelpRenderers[0]?.SetText($"{argName}: {argDesc}");
                }
                else {
                    HelpRenderers[0]?.SetText($"/{matches[0].Name} - {matches[0].Description}");
                }
            }
            else {
                // display all fitting commands
                for (int i = 0; i < displayCount; i++)
                    HelpRenderers[i]?.SetText($"/{matches[i].Name} - {matches[i].Description}");
            }

            // set all other renderers to empty
            for (int i = displayCount; i < HelpRenderers.Length; i++)
                HelpRenderers[i]?.SetText("");
        }
        else {
            foreach (TextRenderer? helper in HelpRenderers)
                helper?.SetText("");
        }
    }


    /// <summary>
    /// Get a list of commands which fit the given string. Only the name of the string is used.
    /// </summary>
    /// <param name="start">The line which was entered. It may be submitted with or without the leading <see cref="CommandPrefix"/>.
    /// Submitted arguments are ignored.</param>
    /// <returns>A list of commands which fit the given start.</returns>
    private IEnumerable<Command> GetMatchingCommandsFromName(string start) {
        if (start.StartsWith(CommandPrefix))
            start = start.Remove(0, CommandPrefix.Length);

        string name = start.Split()[0];

        return Commands.Where(x => x.Name.StartsWith(name, true, null));
    }

    /// <summary>
    /// Gets commands which may be executed on the given line.
    /// </summary>
    /// <param name="line">The line may start with the <see cref="CommandPrefix"/> or may not, the arguments are counted and only fitting 
    /// commands are returned.</param>
    /// <returns>A list with all possible registered commands.</returns>
    private IEnumerable<Command> GetMatchingCommands(string line) {
        int argCount = ExtractArgs(line).Length;
        return GetMatchingCommandsFromName(line).Where(x => x.Arguments.Count == argCount);
    }

    /// <summary>
    /// Extracts the supplied arguments from the input line.
    /// </summary>
    /// <param name="line">The line with the command prefix and the name.</param>
    /// <returns>The unchanged arguments.</returns>
    private string[] ExtractArgs(string line) {
        if (string.IsNullOrWhiteSpace(line))
            return new string[] { };

        return line.Split().Where(x => !string.IsNullOrWhiteSpace(x)).Skip(1).ToArray();
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

        /// <summary>
        /// A short description of the command - is displayed as help for the user.
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// A list with first the name of the argument and second the description what should be input.
        /// </summary>
        public List<(string, string)> Arguments { get; set; } = new();
    }
}