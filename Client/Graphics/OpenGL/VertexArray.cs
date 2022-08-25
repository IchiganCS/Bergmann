using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;

namespace Bergmann.Client.Graphics;

/// <summary>
/// Wraps a vertex array object. Such an object stores all information about vertices required for rendering, such as indices and 
/// vertex attributes.
/// </summary>
public sealed class VertexArray<T> : SafeGlHandle where T : struct {

    /// <summary>
    /// The vertex buffer associated with the vertex array.
    /// </summary>
    public Buffer<T> VertexBuffer { get; init; }


    /// <summary>
    /// The index buffer associated with the vertex array.
    /// </summary>
    public Buffer<uint> IndexBuffer { get; init; }

    /// <summary>
    /// Constructs a new VAO and creates fitting buffers. Those may be accessed at any time through the exposed members.
    /// </summary>
    public VertexArray()
        : this(new(BufferTarget.ArrayBuffer), new(BufferTarget.ElementArrayBuffer)) {

    }

    /// <summary>
    /// Constructs a new object and binds it to the specified buffers. Since a vertex array object stores the names of the buffers,
    /// you can call any operation on the buffer you want and the vao will update automatically. 
    /// They may be also accessed through the exposed members.
    /// </summary>
    /// <param name="vertices">The vertices of the vertex array.</param>
    /// <param name="indices">The indices of the vertex array.</param>
    public VertexArray(Buffer<T> vertices, Buffer<uint> indices) {
        HandleValue = GL.GenVertexArray();
        VertexBuffer = vertices;
        IndexBuffer = indices;

        GL.BindVertexArray(HandleValue);

        vertices.Bind();

        if (typeof(T) == typeof(Vertex3D)) {
            Vertex3D.SetVAOAttributes();
        }
        else if (typeof(T) == typeof(UIVertex)) {
            UIVertex.SetVAOAttributes();
        }
        else {
            // TODO: use c# 11 to make an interface for vertex with a static abstract
            // method to replace this awfulness so that one can than do: T.SetVAOAttributes()
            Logger.Warn($"Invalid generic argument for {nameof(VertexArray<T>)}");
        }


        indices.Bind();
        GL.BindVertexArray(0);
    }

    /// <summary>
    /// Makes a draw call to <see cref="GL.DrawArrays"/> if the handle is valid with the vao bound.
    /// </summary>
    public void Draw() {
        if (IsClosed || IsInvalid) {
            Logger.Warn("Tried drawing invalid vertex array");
            return;
        }

        GL.BindVertexArray(HandleValue);
        GL.DrawElements(PrimitiveType.Triangles, IndexBuffer.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }

    protected override bool ReleaseHandle() {
        VertexBuffer.Close();
        IndexBuffer.Close();
        GlThread.Invoke(() => GL.DeleteVertexArray(HandleValue));
        return true;
    }
}