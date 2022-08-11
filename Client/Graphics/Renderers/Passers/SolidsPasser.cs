using Bergmann.Client.Connectors;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Shared.Objects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.Renderers.Passers;

/// <summary>
/// Renders solid blocks which are not transparent. They don't have to fill the entire block space.
/// </summary>
public class SolidsPasser : IRendererPasser {
    /// <summary>
    /// An array of locks. There are <see cref="_Count"/> many. If one thread holds the lock for i-th entry, 
    /// it may modify the i-th array of <see cref="_VertexArrays"/> and <see cref="_IndexArrays"/>.
    /// </summary>
    private static SemaphoreSlim[] _Locks;
    /// <summary>
    /// The index array. It holds <see cref="_Count"/> many items. 
    /// One may only modify it if the i-th lock of <see cref="_Locks"/> is held.
    /// </summary>
    private static uint[][] _IndexArrays;
    /// <summary>
    /// The index array. It holds <see cref="_Count"/> many items. 
    /// One may only modify it if the i-th lock of <see cref="_Locks"/> is held.
    /// </summary>
    private static Vertex[][] _VertexArrays;

    /// <summary>
    /// How many threads can concurrently work. Keep in mind that these are large arrays, so don't make that number too high.
    /// </summary>
    private static int _Count;

    static SolidsPasser() {
        _Count =  10;

        _Locks = new SemaphoreSlim[_Count];
        _IndexArrays = new uint[_Count][];
        _VertexArrays = new Vertex[_Count][];

        // Construct an estimate maximum. These buffers should never overflow and they are very large.
        // They shouldn't need to be reallocated or garbage collection, that'd be slow
        for (int i = 0; i < _Count; i++) {
            _Locks[i] = new(1, 1);
            _IndexArrays[i] = new uint[Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * 6 * 4 / 2];
            _VertexArrays[i] = new Vertex[Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * 6 * 3 / 2];
        }
    }



    private SortedList<long, SolidsChunkRenderer> Chunkers { get; set; }

    private void MakeNewRendererAt(long key) {
        lock (Chunkers) {
            if (Chunkers.ContainsKey(key)) {
                SolidsChunkRenderer ren = Chunkers[key];
                Task.Run(() => ren.BuildFor(Connection.Active?.Chunks.TryGet(key)!));
            }
            else {
                SolidsChunkRenderer ren = new();
                Chunkers.Add(key, ren);
                Task.Run(() => ren.BuildFor(Connection.Active?.Chunks.TryGet(key)!));
            }
        }
    }

    private void DropRendererAt(long key) {
        lock (Chunkers) {
            if (!Chunkers.ContainsKey(key))
                return;

            Chunkers[key]?.Dispose();
            Chunkers.Remove(key);
        }
    }

    public SolidsPasser() {
        Chunkers = new();
        Connection.Active!.Chunks.OnChunkChanged += (ch, positions) => {
            MakeNewRendererAt(ch.Key);
        };

        Connection.Active!.Chunks.OnChunkAdded += ch => {
            MakeNewRendererAt(ch.Key);
        };

        Connection.Active!.Chunks.OnChunkRemoved += ch =>
            DropRendererAt(ch.Key);
    }


    public void Render() {
        lock (Chunkers)
            foreach (SolidsChunkRenderer ren in Chunkers.Values)
                ren.Render();
    }

    public void Dispose() {
        lock (Chunkers) {
            foreach (SolidsChunkRenderer ren in Chunkers.Values)
                ren.Dispose();

            Chunkers.Clear();
        }
    }



    /// <summary>
    /// A helper class to render all solids in a given chunk.
    /// </summary>
    private class SolidsChunkRenderer {
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
        /// Builds the buffers for a given chunk. Throws away all old buffers. This is quite a costly operation.
        /// </summary>
        /// <param name="chunk">The chunk to build the renderer for.</param>
        public void BuildFor(Chunk chunk) {
            if (chunk is null)
                return;

            int heldLock = -1;

            //look if any lock is free
            for (int i = 0; i < _Count; i++) {
                if (_Locks[i].CurrentCount > 0) {
                    heldLock = i;
                    break;
                }
            }

            //else randomize a lock and just wait for it
            if (heldLock < 0 || heldLock >= _Count)
                heldLock = Random.Shared.Next() % _Count;


            _Locks[heldLock].Wait();

            int currentIndex = 0;
            int currentVertex = 0;


            chunk.ForEach((blockOffset, block) => {
                BlockInfo info = block.Info;
                Vector3i blockPosition = blockOffset + chunk.Offset;

                if (info.ID == 0)
                    return;

                foreach (Geometry.Face face in Geometry.AllFaces) {


                    if (chunk.GetBlockWorld(blockPosition + Geometry.FaceToVector[(int)face]) == 0) {
                        Vector3[] ps = Geometry.Positions[(int)face];
                        int layer = info.GetLayerFromFace(face);
                        Vector3 globalPosition = blockPosition;
                        Vector3 normal = Geometry.FaceToVector[(int)face];

                        _VertexArrays[heldLock][currentVertex + 0] = new() {
                            Position = ps[0] + globalPosition,
                            TexCoord = new(1, 0, layer),
                            Normal = normal,
                        };

                        _VertexArrays[heldLock][currentVertex + 1] = new() {
                            Position = ps[1] + globalPosition,
                            TexCoord = new(1, 1, layer),
                            Normal = normal,
                        };

                        _VertexArrays[heldLock][currentVertex + 2] = new() {
                            Position = ps[2] + globalPosition,
                            TexCoord = new(0, 1, layer),
                            Normal = normal,
                        };

                        _VertexArrays[heldLock][currentVertex + 3] = new() {
                            Position = ps[3] + globalPosition,
                            TexCoord = new(0, 0, layer),
                            Normal = normal,
                        };

                        _IndexArrays[heldLock][currentIndex++] = (uint)currentVertex + 0;
                        _IndexArrays[heldLock][currentIndex++] = (uint)currentVertex + 1;
                        _IndexArrays[heldLock][currentIndex++] = (uint)currentVertex + 2;
                        _IndexArrays[heldLock][currentIndex++] = (uint)currentVertex + 0;
                        _IndexArrays[heldLock][currentIndex++] = (uint)currentVertex + 2;
                        _IndexArrays[heldLock][currentIndex++] = (uint)currentVertex + 3;

                        currentVertex += 4;
                    }
                }
            });

            if (currentIndex == 0) {
                //only air, nothing to render
                _Locks[heldLock].Release();
                return;
            }

            //Write the buffers on the gl thread. Only then release the lock 
            //(hence only one update per frame is possible for now)
            GlThread.Invoke(() => {
                VertexBuffer ??= new Buffer<Vertex>(BufferTarget.ArrayBuffer, currentVertex + 1);
                IndexBuffer ??= new Buffer<uint>(BufferTarget.ElementArrayBuffer, currentIndex + 1);

                IndexBuffer.Fill(_IndexArrays[heldLock], true, currentIndex + 1);
                VertexBuffer.Fill(_VertexArrays[heldLock], true, currentVertex + 1);
                _Locks[heldLock].Release();
                IsRenderable = true;
            });
        }


        /// <summary>
        /// Renders the buffer. It may not quite be up to date, depending on when the other threads finish with their execution, 
        /// but it is guaranteed to either not render anything, or something that was just a little back in time.
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



        public void Dispose() {
            IsRenderable = false;
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }
    }
}