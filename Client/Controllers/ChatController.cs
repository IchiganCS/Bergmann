using Bergmann.Client.Controllers.Modules;
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

    public ChatWriteModule ChatWriter { get; init; }


    /// <summary>
    /// Constructs a new chat controller. It creates a new input field and registeres necessary events.
    /// </summary>
    /// <param name="messageAction">The action to be executed on a normal message. See <see cref="NonCommandAction"/>.</param>
    public ChatController() {
        ChatWriter = new(async msg => {
            if (!string.IsNullOrWhiteSpace(msg))
                await Connection.Active.Send(new ChatMessageSentMessage(msg));

            Stack!.Pop(this);
        });
        Modules.Add(ChatWriter);
    }

    /// <summary>
    /// Forwards typing events to its input field (and special actions) and pops on escape.
    /// </summary>
    /// <param name="updateArgs">The update arguments forwarded to the input field.</param>
    public override void Update(UpdateArgs updateArgs) {
        if (updateArgs.KeyboardState.IsKeyDown(Keys.Escape)) {
            Stack!.Pop(this);
        }

        else {
            base.Update(updateArgs);
            ChatWriter.Update(updateArgs);
        }
    }

    /// <summary>
    /// Render the input field. Later, maybe tooltips or suggestions can be shown too.
    /// </summary>
    public override void Render(RenderUpdateArgs args) {
        ChatWriter.Render();
    }

}