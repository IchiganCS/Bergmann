using Bergmann.Client.InputHandlers;
using Bergmann.Client.Controllers.Modules;
using Bergmann.Shared.Networking;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Bergmann.Client.Graphics.Renderers;
using Bergmann.Client.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers.UI;
using Bergmann.Shared;

namespace Bergmann.Client.Controllers;

/// <summary>
/// Handles the base inputs of the game while the player is moving around etc.
/// </summary>
public class GameController : Controller {

    /// <summary>
    /// Grab the cursor, we only want our cross drawn.
    /// </summary>
    public override CursorState RequestedCursorState => CursorState.Grabbed;

    /// <summary>
    /// The first person handler of the player. It is to be updated when the root game controller is active.
    /// </summary>
    public FPHandler Fph { get; init; }

    private ChatController Chat { get; init; }

    private WorldRenderer WorldRenderer { get; init; }
    private DebugRenderer DebugRenderer { get; init; }
    public bool WireFrameEnabled { get; private set; } = false;
    public bool DebugViewEnabled { get; private set; } = false;

    public GameController() {
        Chat = new();
        Fph = new();

        Fph.Position = (30, 34, 30);


        Chat.Commands.Add(new() {
            Name = "wireframe",
            Execute = x => WireFrameEnabled = !WireFrameEnabled,
        });
        Chat.Commands.Add(new() {
            Name = "recompile",
            Execute = x => GlThread.Invoke(SharedGlObjects.CompilePrograms),
        });
        Chat.Commands.Add(new() {
            Name = "connect",
            Execute = x => {
                Connection.Active = new(x[0]);
            }
        });
        Chat.Commands.Add(new() {
            Execute = args =>
                DebugViewEnabled = !DebugViewEnabled,
            Name = "debug"
        });

        WorldRenderer = new();
        DebugRenderer = new();
        Modules.Add(new WorldLoaderModule());


        InputHandlers.Add(Fph);
    }

    public override void Render(RenderUpdateArgs args) {
        Program.Active = SharedGlObjects.BlockProgram;

        if (WireFrameEnabled)
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        else
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

        Matrix4 viewMat = Fph.LookAt;
        SharedGlObjects.BlockProgram.SetUniform("view", viewMat);
        
        Matrix4 projMat = Matrix4.CreatePerspectiveFieldOfView(1.0f, (float)Graphics.Window.Instance.Size.X / Graphics.Window.Instance.Size.Y, 0.1f, 300f);
        projMat.M11 = -projMat.M11; //this line inverts the x display direction so that it uses our x: LHS >>>>> RHS
        Program.Active.SetUniform("projection", projMat);

        WorldRenderer.Render(new(projMat.Inverted(), Fph.LookAt.Inverted(), 14));

        if (DebugViewEnabled) {
            Program.Active = SharedGlObjects.UIProgram;
            DebugRenderer.Update(Fph.Position, 1f / args.DeltaTime, Connection.Active!.Chunks.Count);
            DebugRenderer.Render();
        }
    }

    public override void Update(UpdateArgs updateArgs) {
        base.Update(updateArgs);

        if (updateArgs.KeyboardState.IsKeyPressed(Keys.F1))
            DebugViewEnabled = !DebugViewEnabled;

        if (updateArgs.KeyboardState.IsKeyPressed(Keys.Enter))
            Stack!.Push(Chat);
    }
}