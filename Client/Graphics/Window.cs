using System.Runtime.InteropServices;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers;
using Bergmann.Shared;
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
        };

        VertexShader.Dispose();
        Fragment.Dispose();
    }

    private Vector3 Camera { get; set; }
    private Vector2 Eulers { get; set; }

    private Texture Dirt { get; set; }

    private WorldRenderer WorldRenderer { get; set; }
    private World World { get; set; }

    private OpenGL.Buffer TestUI { get; set; }

    private Quaternion Rotation =>
        Quaternion.FromEulerAngles(0, Eulers.X, 0) *
        Quaternion.FromEulerAngles(Eulers.Y, 0, 0);


    protected override void OnLoad() {
        VSync = VSyncMode.On;
        CursorState = CursorState.Grabbed;

        MakeProgram();

        BlockInfo.ReadFromJson("Blocks.json");
        BlockRenderer.MakeTextureStack("Textures.json");

        GL.ClearColor(0.0f, 0.0f, 1.0f, 0.0f);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);

        //note that face culling doesn't save runs on the vertex shader 
        //but only on the fragment shader - which still is quite nice to be honest.
        GL.CullFace(CullFaceMode.Back);
        GL.FrontFace(FrontFaceDirection.Ccw);


        World = new World();
        WorldRenderer = new(World);
        World.InitChunks();

        using Image<Rgba32> dirtSide = Image<Rgba32>.Load(ResourceManager.FullPath(ResourceManager.Type.Textures, "dirt_side.jpg")).CloneAs<Rgba32>();

        Dirt = new(TextureTarget.Texture2D);
        Dirt.Write(dirtSide);

        Camera = new(0f, 0f, -3f);
        Eulers = new(20, 40);

        TestUI = new OpenGL.Buffer(BufferTarget.ArrayBuffer);
        TestUI.Fill(
            new UIVertex[3] {
                new() {},
                new() {Percent=new(0, 0.5f)},
                new() {Percent=new(0.5f, 0)}
        });
    }

    protected override void OnUnload() {
        WorldProgram.Dispose();
        UIProgram.Dispose();
        Vertex.CloseVAO();
        UIVertex.CloseVAO();
        BlockRenderer.Dispose();
    }

    protected override void OnUpdateFrame(FrameEventArgs args) {
        if (MouseState.IsButtonDown(MouseButton.Left))
            CursorState = CursorState.Grabbed;

        if (MouseState.IsButtonDown(MouseButton.Right)) {
            //destroy block
            var (pos, face) = World.Raycast(Camera, Rotation * new Vector3(0, 0, 1), out bool hit);
            if (hit) {
                World.SetBlockAt(pos, 0);
            }
        }

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

        Program.Active = WorldProgram;


        GL.ActiveTexture(TextureUnit.Texture0);
        GlLogger.WriteGLError();
        BlockRenderer.TextureStack.Bind();
        GlLogger.WriteGLError();
        GL.Uniform1(GL.GetUniformLocation(WorldProgram.Handle, "stack"), 0);
        GlLogger.WriteGLError();



        WorldRenderer.Render();


        Program.Active = UIProgram;
        TestUI.Bind();
        UIVertex.UseVAO();

        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        GlLogger.WriteGLError();



        Context.SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        GL.Viewport(new System.Drawing.Size(e.Size.X, e.Size.Y));
    }
}