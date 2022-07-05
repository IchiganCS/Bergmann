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
    public Window(GameWindowSettings gameWindowSettings,
                  NativeWindowSettings nativeWindowSettings) : 
        base(gameWindowSettings, nativeWindowSettings) {
    }

    private Program Program { get; set; }
    private Shader VertexShader { get; set; }
    private Shader Fragment { get; set; }
    private OpenGL.Buffer Vertices { get; set; }

    private Vector3 Camera { get; set; }
    private Vector2 Rotation { get; set; }

    protected override void OnLoad() {
        CursorState = CursorState.Grabbed;


        GL.ClearColor(1.0f, 0.0f, 0.0f, 0.0f);

        VertexShader = new(ShaderType.VertexShader);
        Fragment = new(ShaderType.FragmentShader);
        VertexShader.Compile(ResourceManager.ReadFile(ResourceManager.ResourceType.Shaders, "VertexShader.gl"));
        Fragment.Compile(ResourceManager.ReadFile(ResourceManager.ResourceType.Shaders, "FragmentShader.gl"));

        Program = new();
        Program.AddShader(VertexShader);
        Program.AddShader(Fragment);
        Program.Compile();

        Program.Active = Program;

        Vertices = new(BufferTarget.ArrayBuffer);
        Vertices.Fill(new Vertex[] {
            new(){Position = new(0.5f, 0.5f, 0.0f)},
            new(){Position = new(0, 1.0f, 0.0f)},
            new(){Position = new(-0.5f, 0.5f, 0.0f)}
        });

        Vertices.Bind();
        GlLogger.WriteGLError();
        Vertex.UseVAO();

        Camera = new(0f, 0f, -3f);
        Rotation = new(0, 0);
    }

    protected override void OnUnload() {
        Program.Dispose();
        VertexShader.Dispose();
        Fragment.Dispose();
        Vertices.Dispose();
        Vertex.CloseVAO();
    }

    protected override void OnUpdateFrame(FrameEventArgs args) {
        if (MouseState.IsButtonDown(MouseButton.Left))
            CursorState = CursorState.Grabbed;

        if (!IsFocused || CursorState != CursorState.Grabbed)
            return;

        if (KeyboardState.IsKeyDown(Keys.Escape))
            CursorState = CursorState.Normal;



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

        Rotation += MouseState.Delta / 230f * new Vector2(-0.7f, 0.9f);

        while(Math.Abs(Rotation.X) > 3.142)
            Rotation += new Vector2(-MathF.CopySign(2 * MathF.PI, Rotation.X), 0);

        Rotation = new Vector2(Rotation.X, Math.Clamp(Rotation.Y, -1.5f, 1.5f));

        Quaternion rot = Quaternion.FromAxisAngle(new(0, 1, 0), Rotation.X);
        Quaternion rot2 = Quaternion.FromAxisAngle(new(1, 0, 0), Rotation.Y);
        Quaternion combined = rot * rot2;
        float y = delta.Y;
        delta.Y = 0;
        delta = combined * delta;
        Camera += delta;
        Camera += new Vector3(0, y, 0);
    }

    protected override void OnRenderFrame(FrameEventArgs args) {
        GL.Clear(ClearBufferMask.ColorBufferBit);

        int model = GL.GetUniformLocation(Program.Handle, "model");
        int view = GL.GetUniformLocation(Program.Handle, "view");
        int projection = GL.GetUniformLocation(Program.Handle, "projection");

        Matrix4 modelMat = Matrix4.Identity;
        GL.UniformMatrix4(model, false, ref modelMat);

        Quaternion rot = Quaternion.FromAxisAngle(new(0, 1, 0), Rotation.X);
        Quaternion rot2 = Quaternion.FromAxisAngle(new(1, 0, 0), Rotation.Y);
        Quaternion combined = rot * rot2;
        Matrix4 viewMat = Matrix4.LookAt(Camera, Camera + combined * new Vector3(0, 0, 1), new(0, 1, 0));
        GL.UniformMatrix4(view, false, ref viewMat);

        Matrix4 projMat = Matrix4.CreatePerspectiveFieldOfView(1.0f, 4f / 3f, 0.1f, 100f);
        GL.UniformMatrix4(projection, false, ref projMat);

        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

        Context.SwapBuffers();
    }
}