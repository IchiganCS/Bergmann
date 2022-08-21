using Bergmann.Client.Controllers;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Shared.Objects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Bergmann.Client.Graphics;

public class Window : GameWindow {
    /// <summary>
    /// The currently running instance of the window. This might be useful to request window sizes and other values.
    /// </summary>
    public static Window Instance { get; set; } = null!;

    public Window(GameWindowSettings gws, NativeWindowSettings nws) :
        base(gws, nws) {

    }


    /// <summary>
    /// The controller stack always taking care of what to handle. You may think of it as a weird kind of 
    /// scene graph.
    /// </summary>
    private ControllerStack ControllerStack { get; set; } = null!;


    protected override void OnLoad() {
        base.OnLoad();

        BlockInfo.ReadFromJson("Blocks.json");
        ControllerStack = new(new ServiceController());
        SharedGlObjects.BuildAll();


        GlLogger.EnableCallback();


        GL.ClearColor(0.0f, 0.0f, 1.0f, 0.0f);
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);

        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        //note that face culling doesn't save runs on the vertex shader 
        //but only on the fragment shader - which still is quite nice to be honest.
        GL.CullFace(CullFaceMode.Back);
        GL.FrontFace(FrontFaceDirection.Ccw);
    }

    protected override void OnUnload() {
        base.OnUnload();
        SharedGlObjects.FreeAll();
    }

    protected override void OnFocusedChanged(FocusedChangedEventArgs e) {
        base.OnFocusedChanged(e);
        if (e.IsFocused)
            CursorState = ControllerStack.Top.RequestedCursorState;
        else
            CursorState = CursorState.Normal;
    }

    protected override void OnUpdateFrame(FrameEventArgs args) {
        base.OnUpdateFrame(args);


        CursorState = ControllerStack.Top.RequestedCursorState;


        UpdateArgs updateArgs = new((float)args.Time);
        ControllerStack.Update(updateArgs);
    }



    protected override void OnRenderFrame(FrameEventArgs args) {
        base.OnRenderFrame(args);

        GlThread.DoAll();

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        ControllerStack.Render(new RenderUpdateArgs((float)args.Time));
        GlLogger.WriteGLError();

        Context.SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e) {
        base.OnResize(e);
        GL.Viewport(new System.Drawing.Size(e.Size.X, e.Size.Y));
    }
}