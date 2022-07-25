using System.Collections.Concurrent;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Shared;
using Bergmann.Shared.World;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// Renders a chunk. It registers on the events of the specific chunk and updates automatically.
/// This class is used extensively by <see cref="WorldRenderer"/>. It generally works by creating buffers on 
/// the gpu and achieving fast render calls. However, updates are therefore expensive, because the entire buffers 
/// need to be sent to the gpu whenever an update is called for. This class is further optimized to work 
/// with multiple threads. Check the docs to find out.
/// </summary>
public class ChunkRenderer : IDisposable, IRenderer {
    private Chunk Chunk { get; set; }
    private Buffer<Vertex>? VertexBuffer { get; set; }
    private Buffer<uint>? IndexBuffer { get; set; }

    /// <summary>
    /// A lock that allows one thread. We can't use a <see cref="Monitor"/> since it uses weird things as denying operation if
    /// a thread doesn't hold a lock! Who needs that if we know that our code will work?!
    /// </summary>
    private static Semaphore _Lock = new(1, 1);
    private static uint[] _IndexArray = new uint[40000];
    private static Vertex[] _VertexArray = new Vertex[30000];

    /// <summary>
    /// This is a blocking operation. It waits for the lock on the arrays and fills them, then queues an update on the gl thread.
    /// This gl thread then releases the <see cref="_Lock"/>. Thus, only one update per chunk is possible per frame.
    /// </summary>
    private void Update() {
        _Lock.WaitOne();
        List<Vector3i> blocks = Chunk.EveryBlock();

        int currentIndex = 0;
        int currentVertex = 0;

        foreach (Vector3i block in blocks) {
            BlockInfo bi = ((Block)Chunk.Blocks[block.X][block.Y][block.Z]).Info;

            if (bi.ID == 0)
                return;

            foreach (Block.Face face in Block.AllFaces) {

                if (!Chunk.HasNeighborAt(block, face)) {
                    Vector3[] ps = Block.Positions[(int)face];

                    _VertexArray[currentVertex + 0] = new() { Position = ps[0] + block + Chunk.Offset, TexCoord = new(1, 0, bi.GetLayerFromFace(face)) };
                    _VertexArray[currentVertex + 1] = new() { Position = ps[1] + block + Chunk.Offset, TexCoord = new(1, 1, bi.GetLayerFromFace(face)) };
                    _VertexArray[currentVertex + 2] = new() { Position = ps[2] + block + Chunk.Offset, TexCoord = new(0, 1, bi.GetLayerFromFace(face)) };
                    _VertexArray[currentVertex + 3] = new() { Position = ps[3] + block + Chunk.Offset, TexCoord = new(0, 0, bi.GetLayerFromFace(face)) };
                    _IndexArray[currentIndex++] = (uint)currentVertex + 0;
                    _IndexArray[currentIndex++] = (uint)currentVertex + 1;
                    _IndexArray[currentIndex++] = (uint)currentVertex + 2;
                    _IndexArray[currentIndex++] = (uint)currentVertex + 0;
                    _IndexArray[currentIndex++] = (uint)currentVertex + 2;
                    _IndexArray[currentIndex++] = (uint)currentVertex + 3;
                    currentVertex += 4;
                }
            }
        }

        GlThread.Invoke(() => {
            VertexBuffer ??= new Buffer<Vertex>(BufferTarget.ArrayBuffer, currentVertex + 1);
            IndexBuffer ??= new Buffer<uint>(BufferTarget.ElementArrayBuffer, currentIndex + 1);

            IndexBuffer.Fill(_IndexArray, true, currentIndex + 1);
            VertexBuffer.Fill(_VertexArray, true, currentVertex + 1);
            _Lock.Release();
        });
    }

    /// <summary>
    /// This object constructs a new chunk renderer. It may not be ready to be used since the building is done
    /// asynchronously.
    /// </summary>
    /// <param name="chunk">The chunk to be rendered.</param>
    public ChunkRenderer(Chunk chunk) {
        Chunk = chunk;

        Task.Run(Update);
    }


    /// <summary>
    /// Binds all buffers automatically and renders this chunk. The texture stack and the corresponding 
    /// program need to be bound though. This method only will render something if an update has been queued.
    /// </summary>
    public void Render() {
        if (VertexBuffer is not null && IndexBuffer is not null &&
            VertexBuffer.IsFilled && IndexBuffer.IsFilled) {
            VertexBuffer.Bind();
            Vertex.UseVAO();
            IndexBuffer.Bind();
            GlLogger.WriteGLError();

            GL.DrawElements(PrimitiveType.Triangles, IndexBuffer.Length, DrawElementsType.UnsignedInt, 0);
        }
    }

    public void Dispose() {
        VertexBuffer?.Dispose();
        IndexBuffer?.Dispose();
    }
}