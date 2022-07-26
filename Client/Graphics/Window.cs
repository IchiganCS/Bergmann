using Bergmann.Client.Controllers;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers;
using Bergmann.Client.InputHandlers;
using Bergmann.Shared.World;
using Microsoft.AspNetCore.SignalR.Client;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Bergmann.Client.Graphics;

public class Window : GameWindow {

#pragma warning disable CS8618
    public static Window Instance { get; set; }

    public Window(GameWindowSettings gws, NativeWindowSettings nws) :
        base(gws, nws) {

    }
#pragma warning restore CS8618

    private Program BlockProgram { get; set; }
    private Program UIProgram { get; set; }

    private UICollection FixedUIItems { get; set; }
    private DebugRenderer DebugRenderer { get; set; }
    private UICollection ChatItems { get; set; }
    private WorldRenderer WorldRenderer { get; set; }

    private bool WireFrameEnabled { get; set; } = false;

    public ControllerStack ControllerStack { get; set; }
    public FPHandler Fph { get; set; }
    public GameController Controller { get; set; }

    private void MakeProgram() {
        if (BlockProgram is not null)
            BlockProgram.Dispose();

        if (UIProgram is not null)
            UIProgram.Dispose();

        Shader VertexShader = new(ShaderType.VertexShader);
        Shader Fragment = new(ShaderType.FragmentShader);
        VertexShader.Compile(ResourceManager.ReadFile(ResourceManager.Type.Shaders, "Block.vert"));
        Fragment.Compile(ResourceManager.ReadFile(ResourceManager.Type.Shaders, "Block.frag"));

        BlockProgram = new();
        BlockProgram.AddShader(VertexShader);
        BlockProgram.AddShader(Fragment);
        BlockProgram.Compile();
        BlockProgram.OnLoad += () => {
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            if (WireFrameEnabled)
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            else
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            Matrix4 viewMat = Fph.LookAt;
            Matrix4 projMat = Matrix4.CreatePerspectiveFieldOfView(1.0f, (float)Size.X / Size.Y, 0.1f, 300f);
            projMat.M11 = -projMat.M11; //this line inverts the x display direction so that it uses our x: LHS >>>>> RHS
            BlockProgram.SetUniform("projection", projMat);
            BlockProgram.SetUniform("view", viewMat);
            GlLogger.WriteGLError();
        };

        VertexShader.Dispose();
        Fragment.Dispose();

        VertexShader = new(ShaderType.VertexShader);
        Fragment = new(ShaderType.FragmentShader);
        VertexShader.Compile(ResourceManager.ReadFile(ResourceManager.Type.Shaders, "UI.vert"));
        Fragment.Compile(ResourceManager.ReadFile(ResourceManager.Type.Shaders, "UI.frag"));

        UIProgram = new();
        UIProgram.AddShader(VertexShader);
        UIProgram.AddShader(Fragment);
        UIProgram.Compile();

        UIProgram.OnLoad += () => {
            GL.Disable(EnableCap.CullFace);
            GL.Disable(EnableCap.DepthTest); //this is required so that ui elements may overlap
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            UIProgram.SetUniform("windowsize", new Vector2i(Size.X, Size.Y));
            UIProgram.SetUniform("text", 0);
        };

        VertexShader.Dispose();
        Fragment.Dispose();
    }

    private void MakeControllers() {
        ChatController cont = new(x => Hubs.Chat.SendAsync("SendMessage", "ich", x));
        ChatItems = new(null);
        ChatItems.OtherRenderers.Add((new ChatRenderer(cont), true));

        cont.Commands.Add(new() {
            Name = "wireframe",
            Execute = x => WireFrameEnabled = !WireFrameEnabled,
        });
        cont.Commands.Add(new() {
            Name = "recompile",
            Execute = x => MakeProgram(),
        });

        Fph = new FPHandler() {
            Position = (30, 34, 30)
        };
        Controller = new(Fph, cont);
        ControllerStack = new(Controller);
    }

    protected override void OnLoad() {
        VSync = VSyncMode.On;

        MakeControllers();
        MakeProgram();


        BlockInfo.ReadFromJson("Blocks.json");
        BlockRenderer.MakeTextureStack("Textures.json");
        TextRenderer.Initialize();

        GL.ClearColor(0.0f, 0.0f, 1.0f, 0.0f);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        //note that face culling doesn't save runs on the vertex shader 
        //but only on the fragment shader - which still is quite nice to be honest.
        GL.CullFace(CullFaceMode.Back);
        GL.FrontFace(FrontFaceDirection.Ccw);


        WorldRenderer = new();
        WorldRenderer.SubscribeToPositionUpdate(() => Fph.Position);

        Texture UIElems = new Texture(TextureTarget.Texture2DArray);
        UIElems.Reserve(100, 100, 1);

        using Image<Rgba32> img = Image<Rgba32>.Load(
            ResourceManager.FullPath(ResourceManager.Type.Textures, "cross.png")).CloneAs<Rgba32>();
        UIElems.Write(img, 0);


        FixedUIItems = new(UIElems);

        BoxRenderer cross = new(1) {
            AbsoluteAnchorOffset = (0, 0),
            PercentageAnchorOffset = (0.5f, 0.5f),
            RelativeAnchor = (0.5f, 0.5f),
            Dimension = (100, 100)
        };
        cross.ApplyTexture(0);
        FixedUIItems.ImageRenderers.Add((cross, true));

        DebugRenderer = new(() => Fph.Position, () => 40);
    }

    protected override void OnUnload() {
        BlockProgram.Dispose();
        UIProgram.Dispose();
        Vertex.CloseVAO();
        UIVertex.CloseVAO();
        BlockRenderer.Dispose();
        TextRenderer.Delete();
        FixedUIItems.Dispose();
        DebugRenderer.Dispose();
    }

    protected override void OnFocusedChanged(FocusedChangedEventArgs e) {
        if (e.IsFocused)
            CursorState = ControllerStack.Top.RequestedCursorState;
        else
            CursorState = CursorState.Normal;
    }

    protected override void OnUpdateFrame(FrameEventArgs args) {

        if (!IsFocused)
            return;

        CursorState = ControllerStack.Top.RequestedCursorState;


        UpdateArgs updateArgs = new((float)args.Time);
        ControllerStack.Execute(updateArgs);
    }



    protected override void OnRenderFrame(FrameEventArgs args) {
        GlThread.DoAll();

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


        Program.Active = BlockProgram;


        GL.ActiveTexture(TextureUnit.Texture0);
        GlLogger.WriteGLError();
        BlockRenderer.TextureStack.Bind();
        GlLogger.WriteGLError();
        GL.Uniform1(GL.GetUniformLocation(BlockProgram.Handle, "stack"), 0);
        GlLogger.WriteGLError();


        WorldRenderer.Render();


        Program.Active = UIProgram;
        GlLogger.WriteGLError();

        ChatItems.Render();
        FixedUIItems.Render();

        if (Controller.DebugViewEnabled) {
            DebugRenderer.Render();
        }

        Context.SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        GL.Viewport(new System.Drawing.Size(e.Size.X, e.Size.Y));
    }
}