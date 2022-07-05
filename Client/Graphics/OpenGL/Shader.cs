using System.Runtime.CompilerServices;
using OpenTK.Graphics.OpenGL;

namespace Bergmann.Client.Graphics.OpenGL;

public class Shader : IDisposable {
    /// <summary>
    /// The handle for the OpenGl object shader
    /// </summary>
    public int Handle { get; set; }
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
    public void Compile(string fileName) {
        GL.ShaderSource(Handle, ReadSourceCode(fileName));
        GlLogger.WriteGLError();
        GL.CompileShader(Handle);
        GlLogger.WriteGLError();

        IsCompiled = true;
    }

    /// <summary>
    /// Reads the text from a file in the Shaders folder
    /// </summary>
    /// <param name="fileName">The pure and unchanged name of the file.</param>
    /// <param name="filePath">The path of to this source file - this shan't be explicitly given, the compiler fills this</param>
    /// <returns></returns>
    private string ReadSourceCode(string fileName, [CallerFilePath] string? filePath = null) {
        if (filePath is null)
            return string.Empty;

        return File.ReadAllText(Path.Combine(filePath.Remove(filePath.Length - nameof(Shader).Length - 3), "Shaders/" + fileName));
    }

    public void Dispose() {
        GL.DeleteShader(Handle);
        GlLogger.WriteGLError();
        Handle = 0;
    }
}