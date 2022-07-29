using System.Runtime.CompilerServices;
using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;

namespace Bergmann.Client.Graphics.OpenGL;

public class Shader : IDisposable {
    /// <summary>
    /// The handle for the OpenGl object shader
    /// </summary>
    public int Handle { get; private set; }
    /// <summary>
    /// The type of the shaders supported by OpenGl
    /// </summary>
    /// <value></value>
    public ShaderType Type { get; private set; }

    /// <summary>
    /// If the shader is compiled, no more alterations are allowed.
    /// </summary>
    public bool IsCompiled { get; private set; }

    /// <summary>
    /// Initializes a shader of the specified type - once set, this can't be changed later.
    /// </summary>
    /// <param name="type"></param>
    public Shader(ShaderType type) {
        Type = type;
        Handle = GL.CreateShader(Type);
        IsCompiled = false;
        GlLogger.WriteGLError();
    }

    /// <summary>
    /// Compiles a shader found in the Shaders folder
    /// </summary>
    /// <param name="fileName"></param>
    public void Compile(string sourceCode) {
        GL.ShaderSource(Handle, sourceCode);
        GlLogger.WriteGLError();
        GL.CompileShader(Handle);
        GlLogger.WriteGLError();
        string log = GL.GetShaderInfoLog(Handle);
        if (!string.IsNullOrEmpty(log))
            Logger.Warn(log);

        IsCompiled = true;
    }

    public void Dispose() {
        GL.DeleteShader(Handle);
        GlLogger.WriteGLError();
        Handle = 0;
    }
}