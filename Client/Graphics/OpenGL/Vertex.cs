using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.OpenGL;

/// <summary>
/// Represents a vertex for OpenGL
/// </summary>
public struct Vertex {
    public Vector3 Position;
    public Vector3 TexCoord;
    public Vector3 Normal;

    /// <summary>
    /// The handle to a VAO object applicable to all instances of this class.
    /// </summary>-
    public static int Handle { get; set; }

    /// <summary>
    /// Initializes the Handle
    /// </summary>
    private static void InitVAO() {
        Handle = GL.GenVertexArray();
        GL.BindVertexArray(Handle);
        GL.EnableVertexAttribArray(0);        
        GL.EnableVertexAttribArray(1);
        GL.EnableVertexAttribArray(2);
        
        GlLogger.WriteGLError();
    }

    /// <summary>
    /// Binds the VAO specific to this class using the currently bound array buffer
    /// </summary>
    public static void BindVAO() {
        if (Handle == 0)            
            InitVAO();

        GL.BindVertexArray(Handle);

        int size = Marshal.SizeOf<Vertex>();
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, size, Marshal.OffsetOf<Vertex>(nameof(Position)));
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, size, Marshal.OffsetOf<Vertex>(nameof(TexCoord)));
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, size, Marshal.OffsetOf<Vertex>(nameof(Normal)));
        GlLogger.WriteGLError();
    }

    /// <summary>
    /// Disposes the VAO
    /// </summary>
    public static void CloseVAO() {
        GL.DeleteVertexArray(Handle);
        GlLogger.WriteGLError();
        Handle = 0;
    }
}