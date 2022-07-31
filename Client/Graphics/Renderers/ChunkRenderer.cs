using Bergmann.Client.Graphics.OpenGL;
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
    private bool IsRenderable { get; set; } = false;


    /// <summary>
    /// Saves the key of the rendered chunk.
    /// </summary>
    public long ChunkKey { get; private set; }


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
    /// This is a blocking operation, it may be executed on any thread. It waits for the lock on the arrays and fills them, 
    /// then queues an update on the gl thread. This gl thread then releases the <see cref="_Lock"/>. 
    /// Thus, only one update per chunk is possible per frame.
    /// </summary>
    /// <param name="chunk">The chunk to be used for updating. It is used to fill appropriate buffers.</param>
    public void Update(Chunk chunk) {
        _Lock.WaitOne();
        ChunkKey = chunk.Key;
        List<Vector3i> blocks = chunk.EveryBlock();

        int currentIndex = 0;
        int currentVertex = 0;

        foreach (Vector3i blockPosition in blocks) {
            BlockInfo info = ((Block)chunk.Blocks[blockPosition.X, blockPosition.Y, blockPosition.Z]).Info;

            if (info.ID == 0)
                return;

            foreach (Block.Face face in Block.AllFaces) {

                //TODO: what if buffers overflow?
                //further possible improvement: make multiple large arrays to enable working with more than one thread at the same time.
                //possibly required for faster updates since currently locked to one frame per update.

                if (!chunk.HasNeighborAt(blockPosition, face)) {
                    Vector3[] ps = Block.Positions[(int)face];
                    int layer = info.GetLayerFromFace(face);
                    Vector3 globalPosition = blockPosition + chunk.Offset;
                    Vector3 normal = Block.FaceToVector[(int)face];

                    _VertexArray[currentVertex + 0] = new() { 
                        Position = ps[0] + globalPosition, 
                        TexCoord = new(1, 0, layer),
                        Normal = normal,
                    };

                    _VertexArray[currentVertex + 1] = new() { 
                        Position = ps[1] + globalPosition, 
                        TexCoord = new(1, 1, layer),
                        Normal = normal, 
                    };

                    _VertexArray[currentVertex + 2] = new() { 
                        Position = ps[2] + globalPosition, 
                        TexCoord = new(0, 1, layer),
                        Normal = normal, 
                    };

                    _VertexArray[currentVertex + 3] = new() { 
                        Position = ps[3] + globalPosition, 
                        TexCoord = new(0, 0, layer),
                        Normal = normal, 
                    };

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

        if (currentIndex == 0) {
            //only air, nothing to render
            _Lock.Release();
            return;
        }

        //Write the buffers on the gl thread. Only then release the lock (hence only one update per frame is possible for now)
        GlThread.Invoke(() => {
            VertexBuffer ??= new Buffer<Vertex>(BufferTarget.ArrayBuffer, currentVertex + 1);
            IndexBuffer ??= new Buffer<uint>(BufferTarget.ElementArrayBuffer, currentIndex + 1);

            IndexBuffer.Fill(_IndexArray, true, currentIndex + 1);
            VertexBuffer.Fill(_VertexArray, true, currentVertex + 1);
            IsRenderable = true;
            _Lock.Release();
        });
    }

    /// <summary>
    /// This object constructs a new chunk renderer. It is not yet usable, call <see cref="Update"/> on it first.
    /// </summary>
    public ChunkRenderer() {
    }


    /// <summary>
    /// Binds all buffers and renders this chunk. The texture stack and the corresponding 
    /// program need to be bound though. This method only will render something if an update had been queued before.
    /// </summary>
    public void Render() {
        if (IsRenderable) {
            VertexBuffer!.Bind();
            Vertex.BindVAO();
            IndexBuffer!.Bind();
            GlLogger.WriteGLError();

            GL.DrawElements(PrimitiveType.Triangles, IndexBuffer.Length, DrawElementsType.UnsignedInt, 0);
        }
    }

    /// <summary>
    /// Disposes the buffers by the chunk.
    /// </summary>
    public void Dispose() {
        IsRenderable = false;
        VertexBuffer?.Dispose();
        IndexBuffer?.Dispose();
    }
}