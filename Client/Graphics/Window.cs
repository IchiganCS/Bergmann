using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers;
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
    public Window(GameWindowSettings gameWindowSettings,
                  NativeWindowSettings nativeWindowSettings) :
        base(gameWindowSettings, nativeWindowSettings) {

    }
#pragma warning restore CS8618

    private Program WorldProgram { get; set; }
    private Program UIProgram { get; set; }

    private UICollection FixedUIItems{ get; set; }
    private UICollection DebugItems { get; set; }


    private void MakeProgram() {
        if (WorldProgram is not null)
            WorldProgram.Dispose();

        if (UIProgram is not null)
            UIProgram.Dispose();

        Shader VertexShader = new(ShaderType.VertexShader);
        Shader Fragment = new(ShaderType.FragmentShader);
        VertexShader.Compile(ResourceManager.ReadFile(ResourceManager.Type.Shaders, "Box.vert"));
        Fragment.Compile(ResourceManager.ReadFile(ResourceManager.Type.Shaders, "Box.frag"));

        WorldProgram = new();
        WorldProgram.AddShader(VertexShader);
        WorldProgram.AddShader(Fragment);
        WorldProgram.Compile();
        WorldProgram.OnLoad += () => {
            GL.Enable(EnableCap.CullFace);

            Matrix4 viewMat = Matrix4.LookAt(Camera, Camera + Rotation * new Vector3(0, 0, 1), new(0, 1, 0));
            Matrix4 projMat = Matrix4.CreatePerspectiveFieldOfView(1.0f, (float)Size.X / Size.Y, 0.1f, 300f);
            projMat.M11 = -projMat.M11; //this line inverts the x display direction so that it uses our x: LHS >>>>> RHS
            WorldProgram.SetUniform("projection", projMat);
            WorldProgram.SetUniform("view", viewMat);
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

    private Vector3 Camera { get; set; }
    private Vector2 Eulers { get; set; }

    private WorldRenderer WorldRenderer { get; set; }

    private string Text { get; set; }

    private Quaternion Rotation =>
        Quaternion.FromEulerAngles(0, Eulers.X, 0) *
        Quaternion.FromEulerAngles(Eulers.Y, 0, 0);


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


        Camera = new(0f, 0f, -3f);
        Eulers = new(20, 40);

        Texture UIElems = new Texture(TextureTarget.Texture2DArray);
        UIElems.Reserve(100, 100, 1);

        using Image<Rgba32> img = Image<Rgba32>.Load(ResourceManager.FullPath(ResourceManager.Type.Textures, "cross.png")).CloneAs<Rgba32>();
        UIElems.Write(img, 0);


        FixedUIItems = new(UIElems);
        DebugItems = new(null);

        BoxRenderer cross = new(1);
        cross.MakeLayout(new(0, 0), new(0.5f, 0.5f), new(0.5f, 0.5f), new(100, 100), layer: 0);
        FixedUIItems.ImageRenderers.Add((cross, true));

        TextRenderer posText = new("Pos: first", 50, new(30, -30), new(0, 1), new(0, 1));
        DebugItems.OtherRenderers.Add((posText, true));
        TextRenderer text = new("Text: ", 50, new(30, -100), new(0, 1), new(0, 1));
        DebugItems.OtherRenderers.Add((text, true));
        TextRenderer blockText = new("Block: ", 50, new(30, -170), new(0, 1), new(0, 1));
        DebugItems.OtherRenderers.Add((blockText, true));
    }

    protected override void OnUnload() {
        WorldProgram.Dispose();
        UIProgram.Dispose();
        Vertex.CloseVAO();
        UIVertex.CloseVAO();
        BlockRenderer.Dispose();
        TextRenderer.Delete();
        FixedUIItems.Dispose();
        DebugItems.Dispose();
    }

    protected override void OnFocusedChanged(FocusedChangedEventArgs e) {
        if (e.IsFocused)
            CursorState = CursorState.Grabbed;
        else
            CursorState = CursorState.Normal;
    }

    private (Vector3i, Block.Face)? RayCast { get; set; }
    private bool DebugViewEnabled { get; set; } = false;

    protected override void OnUpdateFrame(FrameEventArgs args) {
        if (!IsFocused)
            return;

        if (KeyboardState.IsKeyPressed(Keys.Escape)) {
            if (CursorState == CursorState.Grabbed)
                CursorState = CursorState.Normal;
            else
                CursorState = CursorState.Grabbed;
        }

        if (KeyboardState.IsKeyPressed(Keys.F1))
            DebugViewEnabled = !DebugViewEnabled;

        var (pos, face) = World.Instance.Raycast(Camera, Rotation * new Vector3(0, 0, 1), out bool hit);
        if (hit) {
            RayCast = (pos, face);

            if (MouseState.IsButtonPressed(MouseButton.Left))
                World.Instance.SetBlockAt(RayCast.Value.Item1, 0);
            else if (MouseState.IsButtonPressed(MouseButton.Right))
                World.Instance.SetBlockAt(Block.FaceToVector[(int)RayCast.Value.Item2] + RayCast.Value.Item1, 1);
        }
        else
            RayCast = null;



        if (KeyboardState.IsKeyPressed(Keys.F12))
            MakeProgram();


        if (KeyboardState.IsKeyPressed(Keys.J))
            Text += "j";
        if (KeyboardState.IsKeyPressed(Keys.K))
            Text += "k";
        if (KeyboardState.IsKeyPressed(Keys.L))
            Text += "l";

        if (KeyboardState.IsKeyPressed(Keys.Enter))
            Text = "";


        //movement
        Vector3 delta = new();
        if (KeyboardState.IsKeyDown(Keys.W))
            delta += (float)args.Time * new Vector3(0, 0, 0.8f);
        if (KeyboardState.IsKeyDown(Keys.S))
            delta -= (float)args.Time * new Vector3(0, 0, 0.8f);
        if (KeyboardState.IsKeyDown(Keys.D))
            delta += (float)args.Time * new Vector3(0.8f, 0, 0f);
        if (KeyboardState.IsKeyDown(Keys.A))
            delta -= (float)args.Time * new Vector3(0.8f, 0, 0f);
        if (KeyboardState.IsKeyDown(Keys.Space))
            delta += (float)args.Time * new Vector3(0, 0.8f, 0f);
        if (KeyboardState.IsKeyDown(Keys.LeftControl))
            delta -= (float)args.Time * new Vector3(0, 0.8f, 0f);

        delta *= 4;

        Eulers += MouseState.Delta / 330f * new Vector2(0.7f, 0.9f);

        while (Math.Abs(Eulers.X) > 3.142)
            Eulers += new Vector2(-MathF.CopySign(2 * MathF.PI, Eulers.X), 0);

        Eulers = new Vector2(Eulers.X, Math.Clamp(Eulers.Y, -1.5f, 1.5f));

        float y = delta.Y;
        delta.Y = 0;
        delta = Rotation * delta;
        Camera += delta;
        Camera += new Vector3(0, y, 0);
    }



    protected override void OnRenderFrame(FrameEventArgs args) {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


        Program.Active = WorldProgram;


        GL.ActiveTexture(TextureUnit.Texture0);
        GlLogger.WriteGLError();
        BlockRenderer.TextureStack.Bind();
        GlLogger.WriteGLError();
        GL.Uniform1(GL.GetUniformLocation(WorldProgram.Handle, "stack"), 0);
        GlLogger.WriteGLError();



        WorldRenderer.Render();


        Program.Active = UIProgram;
        GlLogger.WriteGLError();

        if (DebugViewEnabled) {
            (DebugItems.OtherRenderers[0].Item1 as TextRenderer)!.SetText($"Pos: ({Camera.X:0.00}, {Camera.Y:0.00}, {Camera.Z:0.00})");
            (DebugItems.OtherRenderers[1].Item1 as TextRenderer)!.SetText($"Text: {Text}");
            if (RayCast is not null) {
                DebugItems.OtherRenderers[2] = (DebugItems.OtherRenderers[2].Item1, true);
                (DebugItems.OtherRenderers[2].Item1 as TextRenderer)!.SetText($"Block: {World.Instance.GetBlockAt(RayCast.Value.Item1).Info.Name}");
            }
            else
                DebugItems.OtherRenderers[2] = (DebugItems.OtherRenderers[2].Item1, false);
            DebugItems.Render();
        }

        FixedUIItems.Render();

        Context.SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        GL.Viewport(new System.Drawing.Size(e.Size.X, e.Size.Y));
    }
}