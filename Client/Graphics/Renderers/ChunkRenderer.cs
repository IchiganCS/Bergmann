using System.Collections.Concurrent;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Shared;
using Bergmann.Shared.World;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// Renders a chunk. This class is used extensively by <see cref="WorldRenderer"/>. It generally works by creating buffers on 
/// the gpu and achieving fast render calls. However, updates are therefore expensive, because the entire buffers 
/// need to be sent to the gpu whenever an update is called for. This class is further optimized to work 
/// with multiple threads. Check the docs to find out.
/// </summary>
public class ChunkRenderer : IDisposable, IRenderer {

    /// <summary>
    /// A buffer for all vertices on the gpu. It isn't guaranteed to be up to date or even be initialized on time.
    /// Therefore, it's nullable.
    /// </summary>
    private Buffer<Vertex>? VertexBuffer { get; set; }

    /// <summary>
    /// A buffer for all indices on the gpu. It isn't guaranteed to be up to date or even be initialized on time.
    /// Therefore, it's nullable.
    /// </summary>
    private Buffer<uint>? IndexBuffer { get; set; }

    /// <summary>
    /// A boolean whether the current buffer is renderable.
    /// </summary>
    private bool Renderable { get; set; } = false;


    /// <summary>
    /// A lock that allows one thread. We can't use a <see cref="Monitor"/> since it uses weird things as denying operation if
    /// a thread doesn't hold a lock! Who needs that if we know that our code will work?!
    /// </summary>
    private static Semaphore _Lock = new(1, 1);
    /// <summary>
    /// The index array. Should only be accessed with <see cref="_Lock"/> held.
    /// </summary>
    private static uint[] _IndexArray = new uint[40000];
    /// <summary>
    /// The vertex array. Should only be accessed with <see cref="_Lock"/> held.
    /// </summary>
    private static Vertex[] _VertexArray = new Vertex[30000];


    /// <summary>
    /// This is a blocking operation. It waits for the lock on the arrays and fills them, then queues an update on the gl thread.
    /// This gl thread then releases the <see cref="_Lock"/>. Thus, only one update per chunk is possible per frame.
    /// </summary>
    /// <param name="chunk">The chunk to be used for updating. It is used to fill appropriate buffers.</param>
    public void Update(Chunk chunk) {
        _Lock.WaitOne();
        List<Vector3i> blocks = chunk.EveryBlock();

        int currentIndex = 0;
        int currentVertex = 0;

        foreach (Vector3i block in blocks) {
            BlockInfo bi = ((Block)chunk.Blocks[block.X, block.Y, block.Z]).Info;

            if (bi.ID == 0)
                return;

            foreach (Block.Face face in Block.AllFaces) {

                //TODO: what if buffers overflow?
                //further possible improvement: make multiple large arrays to enable working with more than one thread at the same time.
                //possibly required for faster updates since currently locked to one frame per update.

                if (!chunk.HasNeighborAt(block, face)) {
                    Vector3[] ps = Block.Positions[(int)face];

                    _VertexArray[currentVertex + 0] = new() { Position = ps[0] + block + chunk.Offset, TexCoord = new(1, 0, bi.GetLayerFromFace(face)) };
                    _VertexArray[currentVertex + 1] = new() { Position = ps[1] + block + chunk.Offset, TexCoord = new(1, 1, bi.GetLayerFromFace(face)) };
                    _VertexArray[currentVertex + 2] = new() { Position = ps[2] + block + chunk.Offset, TexCoord = new(0, 1, bi.GetLayerFromFace(face)) };
                    _VertexArray[currentVertex + 3] = new() { Position = ps[3] + block + chunk.Offset, TexCoord = new(0, 0, bi.GetLayerFromFace(face)) };
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

        //Write the buffers on the gl thread. Only then release the lock (hence only one update per frame is possible for now)
        GlThread.Invoke(() => {
            VertexBuffer ??= new Buffer<Vertex>(BufferTarget.ArrayBuffer, currentVertex + 1);
            IndexBuffer ??= new Buffer<uint>(BufferTarget.ElementArrayBuffer, currentIndex + 1);

            IndexBuffer.Fill(_IndexArray, true, currentIndex + 1);
            VertexBuffer.Fill(_VertexArray, true, currentVertex + 1);
            Renderable = true;
            _Lock.Release();
        });
    }

    /// <summary>
    /// This object constructs a new chunk renderer. It is not usable.
    /// </summary>
    public ChunkRenderer() {
    }


    /// <summary>
    /// Binds all buffers automatically and renders this chunk. The texture stack and the corresponding 
    /// program need to be bound though. This method only will render something if an update had been queued before.
    /// </summary>
    public void Render() {
        if (Renderable) {
            VertexBuffer!.Bind();
            Vertex.UseVAO();
            IndexBuffer!.Bind();
            GlLogger.WriteGLError();

            GL.DrawElements(PrimitiveType.Triangles, IndexBuffer.Length, DrawElementsType.UnsignedInt, 0);
        }
    }

    public void Dispose() {
        VertexBuffer?.Dispose();
        IndexBuffer?.Dispose();
    }
}