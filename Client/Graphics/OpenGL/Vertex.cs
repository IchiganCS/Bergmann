using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.OpenGL;

/// <summary>
/// Represents a vertex for OpenGL
/// </summary>
public struct Vertex : IBufferData {
    public Vector3 Position;

    /// <summary>
    /// The handle to a VAO object applicable to all instances of this class.
    /// </summary>
    public static int Handle { get; set; }

    /// <summary>
    /// Initializes the Handle
    /// </summary>
    private static void InitVAO() {
        if (Handle != 0)
            return;

        Handle = GL.GenVertexArray();
        GlLogger.WriteGLError();
        GL.BindVertexArray(Handle);
        int size = Marshal.SizeOf<Vertex>();

        GlLogger.WriteGLError();
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, size, Marshal.OffsetOf<Vertex>(nameof(Position)));
        GlLogger.WriteGLError();
        GL.EnableVertexAttribArray(0);
        GlLogger.WriteGLError();
    }

    /// <summary>
    /// Binds the VAO specific to this class
    /// </summary>
    public static void UseVAO() {
        InitVAO();
        GL.BindVertexArray(Handle);
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