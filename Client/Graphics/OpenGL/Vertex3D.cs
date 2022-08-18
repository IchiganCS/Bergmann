using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.OpenGL;

/// <summary>
/// Represents a vertex for OpenGL. It provides static utility methods to set attributes for the layout.
/// </summary>
public struct Vertex3D {
    public Vector3 Position;
    public Vector3 TexCoord;
    public Vector3 Normal;

    private static int Size = Marshal.SizeOf<Vertex3D>();
    private static IntPtr PositionOffset = Marshal.OffsetOf<Vertex3D>(nameof(Position));
    private static IntPtr TexCoordOffset = Marshal.OffsetOf<Vertex3D>(nameof(TexCoord));
    private static IntPtr NormalOffset = Marshal.OffsetOf<Vertex3D>(nameof(Normal));

    /// <summary>
    /// Sets attributes for the currently bound vao to fit this vertex layout.
    /// </summary>
    public static void SetVAOAttributes() {
            GL.EnableVertexAttribArray(0);
            GL.EnableVertexAttribArray(1);
            GL.EnableVertexAttribArray(2);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Size, PositionOffset);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Size, TexCoordOffset);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, Size, NormalOffset);
    }
}