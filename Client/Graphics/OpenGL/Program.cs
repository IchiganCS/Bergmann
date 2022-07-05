using OpenTK.Graphics.OpenGL;

namespace Bergmann.Client.Graphics.OpenGL;

public class Program : IDisposable {
    /// <summary>
    /// Backer for <see cref="Active"/>
    /// </summary>
    private static Program? _Active;
    /// <summary>
    /// Sets the currently running program. It is not possible to set it to null.
    /// Only accepts compiled non-null programs.
    /// </summary>
    public static Program? Active { 
        get => _Active;
        set {
            if (value is null || !value.IsCompiled)
                return;

            GL.UseProgram(value.Handle);
            GlLogger.WriteGLError();
            _Active = value;
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
        if (IsCompiled || !shader.IsCompiled || AttachedShaders.Any(x => x.Type == shader.Type))
            return;

        GL.AttachShader(Handle, shader.Handle);
        GlLogger.WriteGLError();
        AttachedShaders.Add(shader);
    }

    /// <summary>
    /// Compiles the program and therefore locks all alterations
    /// </summary>
    public void Compile() {
        GL.LinkProgram(Handle);
        GlLogger.WriteGLError();
        IsCompiled = true;

        AttachedShaders.ForEach(x => GL.DetachShader(Handle, x.Handle));
        GlLogger.WriteGLError();
    }

    public void Dispose() {
        GL.DeleteProgram(Handle);
        GlLogger.WriteGLError();
        Handle = 0;
    }
}