using System.Runtime.InteropServices;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
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

    protected override void OnLoad() {


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
    }

    protected override void OnUnload() {
        Program.Dispose();
        VertexShader.Dispose();
        Fragment.Dispose();
        Vertices.Dispose();
        Vertex.CloseVAO();
    }

    protected override void OnRenderFrame(FrameEventArgs args) {
        GL.Clear(ClearBufferMask.ColorBufferBit);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        Context.SwapBuffers();
    }
}