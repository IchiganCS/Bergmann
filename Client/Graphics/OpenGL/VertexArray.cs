using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;

namespace Bergmann.Client.Graphics;

/// <summary>
/// Wraps a vertex array object. Such an object stores all information about vertices required for rendering, such as indices and 
/// vertex attributes.
/// </summary>
public sealed class VertexArray<T> : IDisposable where T : struct {
    public int Handle { get; private set; }

    public Buffer<T> VertexBuffer { get; private set; }
    public Buffer<uint> IndexBuffer { get; private set; }

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
    /// <param name="vertices">The vertices </param>
    /// <param name="indices"></param>
    public VertexArray(Buffer<T> vertices, Buffer<uint> indices) {
        Handle = GL.GenVertexArray();
        VertexBuffer = vertices;
        IndexBuffer = indices;

        GL.BindVertexArray(Handle);

        vertices.Bind();

        if (typeof(T) == typeof(Vertex3D)) {
            Vertex3D.SetVAOAttributes();
        }
        else if (typeof(T) == typeof(UIVertex)) {
            UIVertex.SetVAOAttributes();
        }
        else {
            // TODO: use c# 11 to make an interface for vertex with a static abstract
            // method to replace this awfulness
            Logger.Warn($"Invalid generic argument for {nameof(VertexArray<T>)}");
        }


        indices.Bind();
        GL.BindVertexArray(0);
    }

    public void Draw() {
        if (Handle < 0) {
            Logger.Warn("Tried drawing invalid vertex array");
        }

        GL.BindVertexArray(Handle);
        GL.DrawElements(PrimitiveType.Triangles, IndexBuffer.Length, DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);
    }

    public void Dispose() {
        GL.DeleteVertexArray(Handle);
        VertexBuffer.Dispose();
        IndexBuffer.Dispose();
        Handle = -1;
    }
}