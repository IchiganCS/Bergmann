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
    /// An absolute offset in pixel size to the vertex.
    /// </summary>
    public Vector2 Absolute;

    /// <summary>
    /// A 3d value of a texture coordinate. The z component is discarded if this ui vertex disabled texture stack access.
    /// </summary>
    public Vector3 TexCoord;


    private static int Size = Marshal.SizeOf<UIVertex>();
    private static IntPtr PercentOffset = Marshal.OffsetOf<UIVertex>(nameof(Percent));
    private static IntPtr AbsoluteOffset = Marshal.OffsetOf<UIVertex>(nameof(Absolute));
    private static IntPtr TexCoordOffset = Marshal.OffsetOf<UIVertex>(nameof(TexCoord));


    /// <summary>
    /// Sets attributes for the currently bound vao to fit this vertex layout.
    /// </summary>
    public static void SetVAOAttributes() {
        GL.EnableVertexAttribArray(0);
        GL.EnableVertexAttribArray(1);
        GL.EnableVertexAttribArray(2);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Size, PercentOffset);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Size, AbsoluteOffset);
        GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Size, TexCoordOffset);
    }
}