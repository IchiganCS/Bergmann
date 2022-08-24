using System.Runtime.InteropServices;
using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;

namespace Bergmann.Client.Graphics.OpenGL;

/// <summary>
/// Represents any buffer object
/// </summary>
public class Buffer<T> : IDisposable where T : struct {

    /// <summary>
    /// The handle to the OpenGL buffer object
    /// </summary>
    public int Handle { get; private set; }


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
    }

    /// <summary>
    /// Fills the buffer with the appropriate data. The buffer is not updated, if <see cref="items.Length"/> > <see cref="Reserved"/>
    /// </summary>
    /// <param name="items">The new items to be written into the buffer</param>
    /// <param name="reallocate">Tells the object whether to reallocate memory if the given items don't fit in the reserved space.</param>
    /// <param name="length">A possible restriction on how many items to copy from items. Negative values are interpreted to mean
    /// the entire length of the array.</param>
    public void Fill(T[] items, bool reallocate = false, int length = -1) {
        if (Handle <= 0) {
            Logger.Warn("The buffer was already disposed");
            return;
        }

        length = length < 0 ? items.Length : length;
        if (length > Reserved && Reserved > 0 && !reallocate) {
            Logger.Warn("Can't write this many items into the buffer. Aborting");
            return;
        }

        GL.BindBuffer(Target, Handle);

        //checks whether the buffer has already been initalized
        //or if reallocation is necessary
        if (Length <= 0 || (reallocate && length > Reserved)) {

            if (Reserved <= 0 || reallocate)
                Reserved = length;

            //first reserve the buffer. Note that the data parameter is zero, no data is copied
            GL.BufferData(Target, Reserved * Marshal.SizeOf<T>(), IntPtr.Zero, Hint);
        }


        GL.BufferSubData(Target, IntPtr.Zero, length * Marshal.SizeOf<T>(), items);

        Length = length;
    }

    /// <summary>
    /// Binds the buffer and subsequents calls are made on this buffer.
    /// </summary>
    public void Bind() {
        if (Handle <= 0) {
            Logger.Warn("Tried to bind already disposed buffer");
            return;
        }

        GL.BindBuffer(Target, Handle);
        GlLogger.WriteGLError();
    }

    public void Dispose() {
        if (Handle <= 0) {
            Logger.Warn("Tried to dispose already disposed buffer");
            return;
        }
        GL.DeleteBuffer(Handle);
        Handle = -1;
        Length = -1;
        Reserved = -1;
    }
}
