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
    private static Vertex3D[][] _VertexArrays;

    /// <summary>
    /// How many threads can concurrently work. Keep in mind that these are large arrays, so don't make that number too high.
    /// </summary>
    private static int _Count;

    static SolidsPasser() {
        _Count = 10;

        _Locks = new SemaphoreSlim[_Count];
        _IndexArrays = new uint[_Count][];
        _VertexArrays = new Vertex3D[_Count][];

        // Construct an estimate maximum. These buffers should never overflow and they are very large.
        // They shouldn't need to be reallocated or garbage collection, that'd be slow
        for (int i = 0; i < _Count; i++) {
            _Locks[i] = new(1, 1);
            _IndexArrays[i] = new uint[Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * 6 * 4 / 2];
            _VertexArrays[i] = new Vertex3D[Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * Chunk.CHUNK_SIZE * 6 * 3 / 2];
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
            List<long> keys = new();

            MakeNewRendererAt(ch.Key);

            if (positions.Any(x => (x - ch.Offset).Y == 0))
                keys.Add(Chunk.ComputeKey(ch.Offset - (0, 16, 0)));
            if (positions.Any(x => (x - ch.Offset).Y == 15))
                keys.Add(Chunk.ComputeKey(ch.Offset + (0, 16, 0)));
            if (positions.Any(x => (x - ch.Offset).X == 0))
                keys.Add(Chunk.ComputeKey(ch.Offset - (16, 0, 0)));
            if (positions.Any(x => (x - ch.Offset).X == 15))
                keys.Add(Chunk.ComputeKey(ch.Offset + (16, 0, 0)));
            if (positions.Any(x => (x - ch.Offset).Z == 0))
                keys.Add(Chunk.ComputeKey(ch.Offset - (0, 0, 16)));
            if (positions.Any(x => (x - ch.Offset).Z == 15))
                keys.Add(Chunk.ComputeKey(ch.Offset + (0, 0, 16)));

            foreach (var key in keys.Where(Chunkers.ContainsKey))
                MakeNewRendererAt(key);
        };

        Connection.Active!.Chunks.OnChunkAdded += ch => {
            if (Geometry.AllFaces.Select(x => Geometry.FaceToVector[(int)x] * 16 + ch.Offset)
                .Select(Chunk.ComputeKey)
                .All(y => Connection.Active!.Chunks.Any(x => x.Key == y)))

                MakeNewRendererAt(ch.Key);


            foreach (var neighborOffset in Geometry.AllFaces.Select(x => Geometry.FaceToVector[(int)x] * 16 + ch.Offset)) {
                if (Geometry.AllFaces.Select(x => Geometry.FaceToVector[(int)x] * 16 + neighborOffset)
                    .Select(Chunk.ComputeKey)
                    .All(y => Connection.Active!.Chunks.Any(x => x.Key == y)))

                    MakeNewRendererAt(Chunk.ComputeKey(neighborOffset));
            }
        };

        Connection.Active!.Chunks.OnChunkRemoved += ch =>
            DropRendererAt(ch.Key);

        Connection.Active!.Chunks.ForEach(x => MakeNewRendererAt(x.Key));
    }


    public void Render(IrregularBox box) {
        lock (Chunkers)
            foreach (SolidsChunkRenderer ren in Chunkers.Values)
                ren.Render(box);
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
        /// The vao storing all indices, vertices and everything else required to render the chunk.
        /// Note that this object is null for an extended period of time after construction of the renderer.
        /// It is constructed as soon as the <see cref="BuildFor"/> method is called.
        /// </summary>
        private VertexArray<Vertex3D>? VAO { get; set; }

        private Vector3 MiddlePoint { get; set; }


        /// <summary>
        /// Builds the buffers for a given chunk. Throws away all old buffers. This is quite a costly operation.
        /// </summary>
        /// <param name="chunk">The chunk to build the renderer for.</param>
        public void BuildFor(Chunk chunk) {
            if (chunk is null)
                return;

            MiddlePoint = chunk.Offset + (8, 8, 8);

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


                    if (Connection.Active?.Chunks.GetBlockAt(blockPosition + Geometry.FaceToVector[(int)face]) == 0) {
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
                if (VAO is null) {
                    VAO = new(
                        new Buffer<Vertex3D>(BufferTarget.ArrayBuffer, currentVertex + 1),
                        new Buffer<uint>(BufferTarget.ElementArrayBuffer, currentIndex + 1)
                    );
                }

                VAO.IndexBuffer.Fill(_IndexArrays[heldLock], true, currentIndex + 1);
                VAO.VertexBuffer.Fill(_VertexArrays[heldLock], true, currentVertex + 1);
                _Locks[heldLock].Release();
            });
        }


        /// <summary>
        /// Renders the buffer. It may not quite be up to date, depending on when the other threads finish with their execution, 
        /// but it is guaranteed to either not render anything, or something that was just a little back in time.
        /// </summary>
        public void Render(IrregularBox box) {
            if (box.Contains(MiddlePoint))
                VAO?.Draw();
        }



        public void Dispose()
            => VAO?.Dispose();
    }
}