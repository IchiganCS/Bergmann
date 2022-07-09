using System.Runtime.InteropServices;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.OpenGL.Renderers;
using Bergmann.Shared;
using Bergmann.Shared.World;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Shared;

namespace Bergmann.Client.Graphics;

public class Window : GameWindow {
#pragma warning disable CS8618
    public Window(GameWindowSettings gameWindowSettings,
                  NativeWindowSettings nativeWindowSettings) :
        base(gameWindowSettings, nativeWindowSettings) {
            
    }
#pragma warning restore CS8618

    private Program Program { get; set; }
    private void MakeProgram() {
        if (Program is not null)
            Program.Dispose();

        VertexShader = new(ShaderType.VertexShader);
        Fragment = new(ShaderType.FragmentShader);
        VertexShader.Compile(ResourceManager.ReadFile(ResourceManager.ResourceType.Shaders, "Box.vert"));
        Fragment.Compile(ResourceManager.ReadFile(ResourceManager.ResourceType.Shaders, "Box.frag"));

        Program = new();
        Program.AddShader(VertexShader);
        Program.AddShader(Fragment);
        Program.Compile();

        Program.Active = Program;
    }

    private Shader VertexShader { get; set; }
    private Shader Fragment { get; set; }

    private Vector3 Camera { get; set; }
    private Vector2 Eulers { get; set; }

    private WorldRenderer WorldRenderer { get; set; }

    private Quaternion Rotation =>
        Quaternion.FromEulerAngles(0, Eulers.X, 0) *
        Quaternion.FromEulerAngles(Eulers.Y, 0, 0);


    protected override void OnLoad() {
        VSync = VSyncMode.On;
        CursorState = CursorState.Grabbed;

        MakeProgram();

        GL.ClearColor(0.0f, 0.0f, 1.0f, 0.0f);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);

        //note that face culling doesn't save runs on the vertex shader 
        //but only on the fragment shader - which still is quite nice to be honest.
        GL.Enable(EnableCap.CullFace);
        GL.CullFace(CullFaceMode.Back);
        GL.FrontFace(FrontFaceDirection.Ccw);


        World world = new World();
        WorldRenderer = new(world);
        world.InitChunks();

        Camera = new(0f, 0f, -3f);
        Eulers = new(0, 0);
    }

    protected override void OnUnload() {
        Program.Dispose();
        VertexShader.Dispose();
        Fragment.Dispose();
        Vertex.CloseVAO();
    }

    protected override void OnUpdateFrame(FrameEventArgs args) {
        if (MouseState.IsButtonDown(MouseButton.Left))
            CursorState = CursorState.Grabbed;

        if (!IsFocused || CursorState != CursorState.Grabbed)
            return;

        if (KeyboardState.IsKeyDown(Keys.Escape))
            CursorState = CursorState.Normal;

        if (KeyboardState.IsKeyPressed(Keys.Enter))
            MakeProgram();

        if (KeyboardState.IsKeyPressed(Keys.F11)) {
            if (WindowState == WindowState.Normal)
                WindowState = WindowState.Fullscreen;
            else
                WindowState = WindowState.Normal;
        }

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
        

        Matrix4 viewMat = Matrix4.LookAt(Camera, Camera + Rotation * new Vector3(0, 0, 1), new(0, 1, 0));        
        Matrix4 projMat = Matrix4.CreatePerspectiveFieldOfView(1.0f, (float)Size.X / Size.Y, 0.1f, 300f);
        projMat.M11 = -projMat.M11; //this line inverts the x display direction so that it uses our x: LHS >>>>> RHS
        Program.SetUniform("pvm", viewMat * projMat);
        GlLogger.WriteGLError();

        WorldRenderer.Render();

        Context.SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        GL.Viewport(new System.Drawing.Size(e.Size.X, e.Size.Y));
    }
}