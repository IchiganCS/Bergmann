using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.OpenGL;

public struct UIVertex {
    /// <summary>
    /// A percentage (to the the window size) offset to the vertex.
    /// </summary>
    public Vector2 Percent;
    /// <summary>
    /// An absolute offset in pixel size to teh vertex.
    /// </summary>
    public Vector2 Absolute;

    /// <summary>
    /// A 3d value of a texture coordinate. The z component is discarded if this ui vertex disabled texture stack access.
    /// </summary>
    public Vector3 TexCoord;


    /// <summary>
    /// The handle to a VAO object applicable to all instances of this class.
    /// </summary>
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
    public static void UseVAO() {
        if (Handle == 0)
            InitVAO();

        GL.BindVertexArray(Handle);

        int size = Marshal.SizeOf<UIVertex>();
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, size, Marshal.OffsetOf<UIVertex>(nameof(Percent)));
        GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, size, Marshal.OffsetOf<UIVertex>(nameof(Absolute)));
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, size, Marshal.OffsetOf<UIVertex>(nameof(TexCoord)));
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