using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.OpenGL;

public struct UIVertex {
    /// <summary>
    /// The first value 
    /// </summary>
    public Vector2 Percent;
    public Vector2 Absolute;

    public Vector2 TexCoord;


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
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, size, Marshal.OffsetOf<UIVertex>(nameof(TexCoord)));
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