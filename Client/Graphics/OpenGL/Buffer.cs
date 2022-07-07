using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL;

namespace Bergmann.Client.Graphics.OpenGL;

/// <summary>
/// Represents any buffer object
/// </summary>
public class Buffer : IDisposable {
    /// <summary>
    /// The handle to the OpenGL buffer object
    /// </summary>
    public int Handle { get; set; }

    /// <summary>
    /// Indicates whether the buffer holds data. The buffer can only be written once.
    /// </summary>
    private bool Filled { get; set; }

    /// <summary>
    /// The target (=type) of the buffer. It can't be changed after
    /// </summary>
    public BufferTarget Target { get; private set; }

    /// <summary>
    /// The count of elements in the buffer. It returns -1 if the buffer doesn't hold data yet.
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// The type of the elements that were used to fill the buffer. null if the buffer is not filled.
    /// </summary>
    public Type? ItemType { get; private set; }

    /// <summary>
    /// Constructs a new buffer with the specified target
    /// </summary>
    /// <param name="target">The target can't be changed later</param>
    public Buffer(BufferTarget target) {
        Target = target;
        Handle = GL.GenBuffer();
        Filled = false;
        Length = -1;
        ItemType = null;
    }

    /// <summary>
    /// Fills the buffer with the appropriate data. Can only be execute once. T has to be a struct.
    /// </summary>
    public void Fill<T>(T[] items, BufferUsageHint hint = BufferUsageHint.StaticDraw) where T : struct {
        GL.BindBuffer(Target, Handle);
        GlLogger.WriteGLError();
        GL.BufferData(Target, items.Length * Marshal.SizeOf<T>(), items, hint);
        GlLogger.WriteGLError();

        Filled = true;
        ItemType = typeof(T);
        Length = items.Length;
    }

    /// <summary>
    /// Binds the buffer and subsequents calls are made on this buffer.
    /// </summary>
    public void Bind() {
        GL.BindBuffer(Target, Handle);
    }

    public void Dispose() {
        GL.DeleteBuffer(Handle);
        Handle = 0;
    }

}