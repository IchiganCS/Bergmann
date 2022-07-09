using System.Runtime.InteropServices;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Shared;
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
        Fragment.Compile(ResourceManager.ReadFile(ResourceManager.ResourceType.Shaders, "FragmentShader.frag"));

        Program = new();
        Program.AddShader(VertexShader);
        Program.AddShader(Fragment);
        Program.Compile();

        Program.Active = Program;
        
        Matrix4[] models = new Matrix4[6] {
            //many of these transformations could be achieved more easily in some other ways
            //but it matters which side is pointing in which direction
            Matrix4.Identity, //front
            Matrix4.CreateRotationX(MathF.PI / 2), //bottom
            Matrix4.CreateRotationX(MathF.PI / 2) * Matrix4.CreateTranslation(0, 1, 0), //top
            Matrix4.CreateTranslation(-1, 0, -1) * Matrix4.CreateRotationY(MathF.PI), //back
            Matrix4.CreateTranslation(-1, 0, 0) * Matrix4.CreateRotationY(MathF.PI / 2), //right
            Matrix4.CreateTranslation(0, 0, -1) * Matrix4.CreateRotationY(-MathF.PI / 2) //left
        };

        Program.SetUniforms("models", models);

        Vector3[] positions = new Vector3[16 * 16 * 16];
        for (int i = 0; i < 16; i++)
            for (int j = 0; j < 16; j++)
                for (int k = 0; k < 16; k++)
                    positions[i * 16 * 16 + j * 16 + k] = new(i, j, k);

        BlockPositions = new OpenGL.Buffer(BufferTarget.UniformBuffer);
        BlockPositions.Fill(positions);

        int bbPoint = 1;
        int loc = GL.GetUniformBlockIndex(Program.Handle, "blockPositions");
        GlLogger.WriteGLError();
        GL.UniformBlockBinding(Program.Handle, loc, bbPoint);
        GlLogger.WriteGLError();
        GL.BindBufferBase(BufferRangeTarget.UniformBuffer, bbPoint, BlockPositions.Handle);
        GlLogger.WriteGLError();

    }

    private Shader VertexShader { get; set; }
    private Shader Fragment { get; set; }
    private OpenGL.Buffer Vertices { get; set; }
    private OpenGL.Buffer Indices { get; set; }

    private Vector3 Camera { get; set; }
    private Vector2 Eulers { get; set; }

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

        Vertices = new(BufferTarget.ArrayBuffer);
        Vertices.Fill(new Vertex[] {
            new(){Position = new(1f, 0f, 0.0f)},
            new(){Position = new(1f, 1f, 0.0f)},
            new(){Position = new(0, 1f, 0.0f)},
            new(){Position = new(0, 0f, 0.0f)}
        });

        Indices = new(BufferTarget.ElementArrayBuffer);
        Indices.Fill(new int[] {
            0, 1, 2,
            0, 2, 3
        });

        Vertices.Bind();
        GlLogger.WriteGLError();
        Vertex.UseVAO();

        Camera = new(0f, 0f, -3f);
        Eulers = new(0, 0);
    }

    protected override void OnUnload() {
        Program.Dispose();
        VertexShader.Dispose();
        Fragment.Dispose();
        Vertices.Dispose();
        Indices.Dispose();
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
            delta -= (float)args.Time * new Vector3(0.8f, 0, 0f);
        if (KeyboardState.IsKeyDown(Keys.A))
            delta += (float)args.Time * new Vector3(0.8f, 0, 0f);
        if (KeyboardState.IsKeyDown(Keys.Space))
            delta += (float)args.Time * new Vector3(0, 0.8f, 0f);
        if (KeyboardState.IsKeyDown(Keys.LeftControl))
            delta -= (float)args.Time * new Vector3(0, 0.8f, 0f);

        delta *= 4;

        Eulers += MouseState.Delta / 330f * new Vector2(-0.7f, 0.9f);

        while (Math.Abs(Eulers.X) > 3.142)
            Eulers += new Vector2(-MathF.CopySign(2 * MathF.PI, Eulers.X), 0);

        Eulers = new Vector2(Eulers.X, Math.Clamp(Eulers.Y, -1.5f, 1.5f));

        float y = delta.Y;
        delta.Y = 0;
        delta = Rotation * delta;
        Camera += delta;
        Camera += new Vector3(0, y, 0);

        
    }

    private OpenGL.Buffer BlockPositions;

    protected override void OnRenderFrame(FrameEventArgs args) {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        

        Matrix4 modelMat = Matrix4.Identity;
        Program.SetUniform("model", modelMat);

        Matrix4 viewMat = Matrix4.LookAt(Camera, Camera + Rotation * new Vector3(0, 0, 1), new(0, 1, 0));
        Program.SetUniform("view", viewMat);

        
        Matrix4 projMat = Matrix4.CreatePerspectiveFieldOfView(1.0f, (float)Size.X / Size.Y, 0.1f, 100f);
        Program.SetUniform("projection", projMat);

        Vertices.Bind();
        Indices.Bind();
        BlockPositions.Bind();

        //each instance draws a side
        GL.DrawElementsInstanced(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, IntPtr.Zero, 4096 * 6);
        GlLogger.WriteGLError();

        Context.SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        GL.Viewport(new System.Drawing.Size(e.Size.X, e.Size.Y));
    }
}