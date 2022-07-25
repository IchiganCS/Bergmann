using System.Runtime.InteropServices;
using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;

namespace Bergmann.Client.Graphics.OpenGL;

/// <summary>
/// Represents any buffer object
/// </summary>
public class Buffer<T> : IDisposable where T : struct {

    public static int ActiveBuffer { get; set; } = 0;

    /// <summary>
    /// The handle to the OpenGL buffer object
    /// </summary>
    public int Handle { get; set; }


    /// <summary>
    /// The target (=type) of the buffer. It can't be changed after construction.
    /// </summary>
    public BufferTarget Target { get; init; }

    /// <summary>
    /// How many times the buffer is expected to change
    /// </summary>
    public BufferUsageHint Hint { get; init; }

    /// <summary>
    /// The count of elements in the buffer. It returns -1 if the buffer doesn't hold data yet. This is a count int items of <see cref="T"/>
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// How many items can fit in the buffer. Can't be changed after construction. This is a count in items of <see cref="T"/>
    /// </summary>
    public int Reserved { get; private set; }

    /// <summary>
    /// If the buffer was ever filled. This doesn't mean, that the buffer holds data, but it's then filled with any data.
    /// </summary>
    public bool IsFilled
        => Length >= 0;

    /// <summary>
    /// Constructs a new buffer with the specified target
    /// </summary>
    /// <param name="target">The target can't be changed later</param>
    /// <param name="count">If a count is given, </param>
    public Buffer(BufferTarget target, int count = -1, BufferUsageHint hint = BufferUsageHint.StaticDraw) {
        Target = target;
        Handle = GL.GenBuffer();
        Reserved = count;
        Length = -1;
        Hint = hint;
        GlLogger.WriteGLError();
    }

    /// <summary>
    /// Fills the buffer with the appropriate data. The buffer is not updated, if <see cref="items.Length"/> > <see cref="Reserved"/>
    /// </summary>
    /// <param name="items">The new items to be written into the buffer</param>
    /// <param name="reallocate">Tells the object whether to reallocate memory if the given items don't fit in the reserved space.</param>
    public void Fill(T[] items, bool reallocate = false, int length = -1) {
        length = length < 0 ? items.Length : length;
        if (length > Reserved && Reserved > 0 && !reallocate) {
            Logger.Warn("Can't write this many items into the buffer. Aborting");
            return;
        }

        GL.BindBuffer(Target, Handle);
        Console.WriteLine($"Written {length * Marshal.SizeOf<T>()} bytes");

        //checks whether the buffer has already been initalized
        //or if reallocation is necessary
        if (Length <= 0 || (reallocate && length > Reserved)) {

            if (Reserved <= 0 || reallocate)
                Reserved = length;

            //first reserve the buffer. Note that the data parameter is zero, no data is copied
            GL.BufferData(Target, Reserved * Marshal.SizeOf<T>(), IntPtr.Zero, Hint);
        }


        GL.BufferSubData(Target, IntPtr.Zero, length * Marshal.SizeOf<T>(), items);

        GlLogger.WriteGLError();

        Length = length;
    }

    /// <summary>
    /// Binds the buffer and subsequents calls are made on this buffer.
    /// </summary>
    public void Bind() {
        GL.BindBuffer(Target, Handle);
        GlLogger.WriteGLError();
    }

    public void Dispose() {
        GL.DeleteBuffer(Handle);
        Handle = -1;
    }

}