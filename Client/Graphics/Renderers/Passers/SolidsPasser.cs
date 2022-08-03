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
    /// A lock that allows one thread. We can't use a <see cref="Monitor"/> since it uses weird things as denying operation if
    /// a thread doesn't hold a lock! Who needs that if we know that our code will work?!
    /// </summary>
    private static Semaphore _Lock = new(1, 1);
    /// <summary>
    /// The index array. Should only be accessed with <see cref="_Lock"/> held.
    /// </summary>
    private static uint[] _IndexArray = new uint[Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * 6 * 4];
    /// <summary>
    /// The vertex array. Should only be accessed with <see cref="_Lock"/> held.
    /// </summary>
    private static Vertex[] _VertexArray = new Vertex[Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * 6 * 3];



    private SortedList<long, SolidsChunkRenderer> Chunkers { get; set; }

    private void MakeNewRendererAt(long key) {
        lock (Chunkers) {
            if (Chunkers.ContainsKey(key)) {
                SolidsChunkRenderer ren = Chunkers[key];
                Task.Run(() => ren.BuildFor(Data.Chunks.Get(key)!));
            }
            else {
                SolidsChunkRenderer ren = new();
                Chunkers.Add(key, ren);
                Task.Run(() => ren.BuildFor(Data.Chunks.Get(key)!));
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
        Data.Chunks.OnChunkChanged += (ch, positions) => {
            MakeNewRendererAt(ch.Key);
        };

        Data.Chunks.OnChunkAdded += ch => {
            MakeNewRendererAt(ch.Key);
        };

        Data.Chunks.OnChunkRemoved += ch =>
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

        public void BuildFor(Chunk chunk) {
            if (chunk is null)
                return;

            _Lock.WaitOne();

            int currentIndex = 0;
            int currentVertex = 0;


            chunk.ForEach((blockOffset, block) => {
                BlockInfo info = block.Info;
                Vector3i blockPosition = blockOffset + chunk.Offset;

                if (info.ID == 0)
                    return;

                foreach (Geometry.Face face in Geometry.AllFaces) {

                    //TODO: what if buffers overflow?
                    //further possible improvement: make multiple large arrays to enable working with more than one thread at the same time.
                    //possibly required for faster updates since currently locked to one frame per update.

                    if (chunk.GetBlockWorld(blockPosition + Geometry.FaceToVector[(int)face]) == 0) {
                        Vector3[] ps = Geometry.Positions[(int)face];
                        int layer = info.GetLayerFromFace(face);
                        Vector3 globalPosition = blockPosition;
                        Vector3 normal = Geometry.FaceToVector[(int)face];

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
            });

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