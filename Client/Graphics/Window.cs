using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers;
using Bergmann.Client.InputHandlers;
using Bergmann.Shared.World;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Bergmann.Client.Graphics;

public class Window : GameWindow {
    #pragma warning disable CS8618
    public Window(GameWindowSettings gws, NativeWindowSettings nws) :
        base(gws, nws) {

    }
    #pragma warning restore CS8618

    private Program BlockProgram { get; set; }
    private Program UIProgram { get; set; }

    private UICollection FixedUIItems{ get; set; }
    private UICollection DebugItems { get; set; }
    private UICollection ChatItems{ get; set; }
    private WorldRenderer WorldRenderer { get; set; }

    private FPSController FPS { get; set; }
    private (Vector3i, Block.Face)? RayCast { get; set; }
    private bool DebugViewEnabled { get; set; } = false;
    private bool WireFrameEnabled { get; set; } = false;

    private bool Chatting { get; set; } = false;
    private TextField ChatField { get; set; } = new();


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

            Matrix4 viewMat = FPS.LookAt();
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

            UIProgram.SetUniform("windowsize", new Vector2i(Size.X, Size.Y));
            UIProgram.SetUniform("text", 0);
        };

        VertexShader.Dispose();
        Fragment.Dispose();
    }



    protected override void OnLoad() {
        VSync = VSyncMode.On;

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


        FPS = new() {
            Position = new(8, 35, 8)
        };

        Texture UIElems = new Texture(TextureTarget.Texture2DArray);
        UIElems.Reserve(100, 100, 1);

        using Image<Rgba32> img = Image<Rgba32>.Load(ResourceManager.FullPath(ResourceManager.Type.Textures, "cross.png")).CloneAs<Rgba32>();
        UIElems.Write(img, 0);


        FixedUIItems = new(UIElems);
        DebugItems = new(null);
        ChatItems = new(null);

        BoxRenderer cross = new(1) {
            AbsoluteAnchorOffset = (0, 0), 
            PercentageAnchorOffset = (0.5f, 0.5f), 
            RelativeAnchor = (0.5f, 0.5f), 
            Dimension = (100, 100)
        };
        cross.ApplyTexture(0);
        FixedUIItems.ImageRenderers.Add((cross, true));

        TextRenderer posText = new() {
            AbsoluteAnchorOffset = (30, -30), 
            PercentageAnchorOffset = (0, 1), 
            RelativeAnchor = (0, 1),
            Dimension = (-1, 50)
        };        
        DebugItems.OtherRenderers.Add((posText, true));
        TextRenderer blockText = new() {
            AbsoluteAnchorOffset = (30, -100),
            PercentageAnchorOffset = (0, 1),
            RelativeAnchor = (0, 1),
            Dimension = (-1, 50)
        };
        DebugItems.OtherRenderers.Add((blockText, true));

        TextRenderer chatText = new() {
            RelativeAnchor = (0, 0),
            AbsoluteAnchorOffset = (50, 50),
            PercentageAnchorOffset = (0, 0),
            Dimension = (-1, 50)
        };
        ChatItems.OtherRenderers.Add((chatText, false));
    }

    protected override void OnUnload() {
        BlockProgram.Dispose();
        UIProgram.Dispose();
        Vertex.CloseVAO();
        UIVertex.CloseVAO();
        BlockRenderer.Dispose();
        TextRenderer.Delete();
        FixedUIItems.Dispose();
        DebugItems.Dispose();
    }

    protected override void OnUpdateFrame(FrameEventArgs args) {
        base.OnUpdateFrame(args);

        if (!IsFocused)
            return;

        if (KeyboardState.IsKeyPressed(Keys.Escape)) {
            if (CursorState == CursorState.Grabbed)
                CursorState = CursorState.Normal;
            else
                CursorState = CursorState.Grabbed;
        }

        if (CursorState != CursorState.Grabbed)
            return;

        if (KeyboardState.IsKeyPressed(Keys.Enter)) {
            Chatting = !Chatting;
            if (Chatting) {
                //ChatText = TextField.GetTypedString(ChatText, KeyboardState);
                TextInput += ChatField.HandleTextInput;
            }
            else {
                TextInput -= ChatField.HandleTextInput;
            }
        }
        if (Chatting) {
            ChatField.HandleCursor(KeyboardState);
            return;
        }


        if (KeyboardState.IsKeyPressed(Keys.F1))
            DebugViewEnabled = !DebugViewEnabled;

        if (KeyboardState.IsKeyPressed(Keys.F11)) {
            WireFrameEnabled = !WireFrameEnabled;
            if (WireFrameEnabled)
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
            else
                GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        if (KeyboardState.IsKeyPressed(Keys.F12))
            MakeProgram();

        var (pos, face) = World.Instance.Raycast(FPS.Position, FPS.Rotation * new Vector3(0, 0, 1), out bool hit);
        if (hit) {
            RayCast = (pos, face);

            if (MouseState.IsButtonPressed(MouseButton.Left))
                World.Instance.SetBlockAt(RayCast.Value.Item1, 0);
            else if (MouseState.IsButtonPressed(MouseButton.Right))
                World.Instance.SetBlockAt(Block.FaceToVector[(int)RayCast.Value.Item2] + RayCast.Value.Item1, 1);
        }
        else
            RayCast = null;



        FPS.RotateCamera(MouseState.Delta);
        FPS.FlyingMovement((float)args.Time, KeyboardState);
        World.Instance.EnsureChunksLoaded(FPS.Position, 6);
    }



    protected override void OnRenderFrame(FrameEventArgs args) {
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

        if (DebugViewEnabled) {
            (DebugItems.OtherRenderers[0].Item1 as TextRenderer)!.SetText($"Pos: ({FPS.Position.X:0.00}, {FPS.Position.Y:0.00}, {FPS.Position.Z:0.00})");
            if (RayCast is not null) {
                DebugItems.OtherRenderers[1] = (DebugItems.OtherRenderers[1].Item1, true);
                (DebugItems.OtherRenderers[1].Item1 as TextRenderer)!.SetText($"Block: {World.Instance.GetBlockAt(RayCast.Value.Item1).Info.Name}, {RayCast.Value.Item2}");
            }
            else
                DebugItems.OtherRenderers[1] = (DebugItems.OtherRenderers[1].Item1, false);
            DebugItems.Render();
        }

        if (!string.IsNullOrEmpty(ChatField.Value)) {
            (ChatItems.OtherRenderers[0].Item1 as TextRenderer)!.SetText(ChatField.Value);
            ChatItems.OtherRenderers[0] = (ChatItems.OtherRenderers[0].Item1, true);
            ChatItems.Render();
        }
        else {

        }


        FixedUIItems.Render();

        Context.SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        GL.Viewport(new System.Drawing.Size(e.Size.X, e.Size.Y));
    }
}