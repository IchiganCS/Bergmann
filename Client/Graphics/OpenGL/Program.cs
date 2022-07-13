using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.OpenGL;

public class Program : IDisposable {
    /// <summary>
    /// Backer for <see cref="Active"/>
    /// </summary>
    private static Program? _Active;
    /// <summary>
    /// Sets the currently running program. It is not possible to set it to null.
    /// Only accepts compiled non-null programs. Calls the <see cref="OnLoad"/> and <see cref="OnUnload"/>
    /// events when appropriate. The events are not invoked, if the program is already active.
    /// </summary>
    public static Program? Active { 
        get => _Active;
        set {
            if (value is null || !value.IsCompiled)
                return;

            if (_Active is not null) {
                if (_Active.Handle == value.Handle)
                    return;

                _Active.OnUnload?.Invoke();
            }

            GL.UseProgram(value.Handle);
            GlLogger.WriteGLError();
            _Active = value;

            _Active.OnLoad.Invoke();
        } 
    }


    /// <summary>
    /// The handle to the specific program handle by OpenGL
    /// </summary>
    public int Handle { get; set; }
    /// <summary>
    /// Whether the program is already compiled. When the program is compiled, there are no more changes allowed.
    /// </summary>
    public bool IsCompiled { get; private set; }

    /// <summary>
    /// A list of already attached shaders, useful for detaching later
    /// </summary>
    private List<Shader> AttachedShaders { get; set; }

    public Program() {
        Handle = GL.CreateProgram();
        AttachedShaders = new();
        IsCompiled = false;
    }

    /// <summary>
    /// Rejects the operation if
    /// 1) the program is already compiled
    /// 2) the shader is not compiled
    /// 3) the type of the shader was already attached.
    /// 
    /// Submitted shaders are not disposed.
    /// </summary>
    /// <param name="shader">Has to be a ready to use, already compiled shader</param>
    public void AddShader(Shader shader) {
        if (IsCompiled || !shader.IsCompiled || AttachedShaders.Any(x => x.Type == shader.Type)) {
            Logger.Warn("Shader could not be attached");
            return;
        }

        GL.AttachShader(Handle, shader.Handle);
        GlLogger.WriteGLError();
        AttachedShaders.Add(shader);
    }

    /// <summary>
    /// Compiles the program and therefore locks all alterations
    /// </summary>
    public void Compile() {
        if (IsCompiled) {
            Logger.Warn("Tried to recompile a program");
            return;
        }

        GL.LinkProgram(Handle);
        GlLogger.WriteGLError();
        IsCompiled = true;

        AttachedShaders.ForEach(x => GL.DetachShader(Handle, x.Handle));
        GlLogger.WriteGLError();
    }

    /// <summary>
    /// Provides a unified way to pass arguments as a uniform variable. This is slower than setting it by hand.
    /// </summary>
    /// <param name="name">The name used in the shader for the variable</param>
    /// <param name="item">The item of type T</param>
    /// <typeparam name="T">Though every type supported by GL.Uniform<...> is theoretically possible, only some are implemented</typeparam>
    public void SetUniform<T>(string name, T item) {
        if (!IsCompiled) {
            Logger.Warn("Tried to set a uniform for a non compiled program");
            return;
        }
        if (Active?.Handle != this.Handle) {
            Logger.Warn("Cannot bind uniform for non-active program");
            return;
        }

        int pos = GL.GetUniformLocation(Handle, name);
        if (pos < 0) {
            Logger.Warn("Cannot find location for " + name);
            return;
        }

        if (item is Matrix4 matrix4)
            GL.UniformMatrix4(pos, false, ref matrix4);

        else if (item is Vector3 vector3)
            GL.Uniform3(pos, ref vector3);

        else if (item is Vector2i vector2i)
            GL.Uniform2(pos, ref vector2i);

        else
            Logger.Warn($"Couldn't bind uniform {name} for type {typeof(T).ToString()}. Unsupported type?");
    }
    /// <summary>
    /// Wraps all necessary calls for <see cref="SetUniform"/> needed to fill an array of uniforms. See that function for more detail.
    /// </summary>
    /// <param name="name">The name without any brackets.</param>
    public void SetUniforms<T>(string name, T[] items) {
        for (int i = 0; i < items.Length; i++)
            SetUniform($"{name}[{i}]", items[i]);
    }

    /// <summary>
    /// Is called when the program is set as active. You can call whatever functions are necessary.
    /// The main use case is to make program specific GL calls.
    /// </summary>
    public event LoadDelegate OnLoad = default!;
    public delegate void LoadDelegate();



    /// <summary>
    /// Is called when the shader is unloaded. Can be used to restore the OpenGL state if necessary.
    /// </summary>
    public event UnloadDelegate OnUnload = default!;
    public delegate void UnloadDelegate();


    public void Dispose() {
        GL.DeleteProgram(Handle);
        GlLogger.WriteGLError();
        Handle = 0;
    }
}