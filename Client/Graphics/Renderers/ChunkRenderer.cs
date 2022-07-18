using System.Collections.Concurrent;
using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Shared.World;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// Renders a chunk. It registers on the events of the specific chunk and updates automatically.
/// This class is used extensively by <see cref="WorldRenderer"/>. It generally works by creating buffers on the gpu and achieving fast render calls.
/// However, updates are therefore expensive, because the entire buffers need to be sent to the gpu whenever an update is called for.
/// This class can be further optimized to work with multiple threads.
/// </summary>
public class ChunkRenderer : IDisposable, IRenderer {
    private Chunk Chunk { get; set; }
    private Buffer<Vertex> VertexBuffer { get; set; }
    private Buffer<uint> IndexBuffer { get; set; }

    private Vertex[] ContiguousVerticesCache { get; set; }
    private uint[] ContiguousIndicesCache { get; set; }
    private bool ContiguousCacheUpToDate{ get; set; }
    private bool BuffersUpToDate { get; set; }

    /// <summary>
    /// The key is the block position given by x * 16 * 16 + y * 16 + z. The pair stores each rendered vertex of the block
    /// with the appropriate properties, the uint array stores the indices for these vertices. These indices are local for their key, there are many doubles overall
    /// </summary>
    private ConcurrentDictionary<int, (Vertex[], uint[])> Cache { get; set; }

    #pragma warning disable CS8618
    public ChunkRenderer() {
        VertexBuffer = new Buffer<Vertex>(BufferTarget.ArrayBuffer, 13000);
        IndexBuffer = new Buffer<uint>(BufferTarget.ElementArrayBuffer, 13000);
    }
    #pragma warning restore CS8618

    public void InitWith(Chunk chunk) {
        Chunk = chunk;

        Chunk.OnUpdate += Update;

        ContiguousCacheUpToDate = false;
        BuffersUpToDate = false;

        BuildCache();
        UpdateContiguousCache();
    }

    /// <summary>
    /// Looks up each block and each face and loads the Cache. This is a very costly operation and should 
    /// preferabbly only executed once.
    /// </summary>
    private void BuildCache() {
        Cache = new();

        var blocks = Chunk.EveryBlock();


        foreach (Vector3i block in blocks) {
            UpdateCacheAt(block);
        }
    }

    /// <summary>
    /// Updates the cache at a specific position.
    /// </summary>
    /// <param name="position">The position in chunk space</param>
    private void UpdateCacheAt(Vector3i block) {
        ContiguousCacheUpToDate = false;
        BuffersUpToDate = false;

        BlockInfo bi = ((Block)Chunk.Blocks[block.X, block.Y, block.Z]).Info;
        int key = block.X * 16 * 16 + block.Y * 16 + block.Z;
        Cache.Remove(key, out _);

        if (bi.ID == 0)
            return;

        foreach (Block.Face face in Block.AllFaces) {

            if (!Chunk.HasNeighborAt(block, face)) {
                //we have to add it to the dicitionary

                Vector3[] ps = Block.Positions[(int)face];
                Vertex[] positions = new Vertex[4] {
                    new() { Position = ps[0] + block + Chunk.Offset, TexCoord = new(1, 0, bi.GetLayerFromFace(face)) },
                    new() { Position = ps[1] + block + Chunk.Offset, TexCoord = new(1, 1, bi.GetLayerFromFace(face)) },
                    new() { Position = ps[2] + block + Chunk.Offset, TexCoord = new(0, 1, bi.GetLayerFromFace(face)) },
                    new() { Position = ps[3] + block + Chunk.Offset, TexCoord = new(0, 0, bi.GetLayerFromFace(face)) }
                };

                if (Cache.ContainsKey(key)) {
                    var old = Cache[key];
                    List<Vertex> newVertex = old.Item1.ToList();
                    newVertex.AddRange(positions);
                    List<uint> newIndices = old.Item2.ToList();
                    newIndices.AddRange(Block.Indices.Select(x => x + (uint)old.Item1.Length));
                    Cache[key] = (newVertex.ToArray(), newIndices.ToArray());
                }
                else {
                    Cache.AddOrUpdate(key, (positions, Block.Indices), (a, b) => b);
                }
            }
        }
    }


    /// <summary>
    /// Updates the cache at specific points, since rebuilding the entire cache is too expensive. This is an optimization though, theoretically
    /// rebuilding the entire cache works. This is to be registered as a callback for the <see cref="Chunk.OnUpdate"/> event.
    /// </summary>
    /// <param name="positions"></param>
    private void Update(List<Vector3i> positions) {
        List<Vector3i> all = new();

        all.AddRange(positions);
        foreach (IEnumerable<Vector3i> neighbors in positions.Select(x => Block.AllNeighbors(x)))
            all.AddRange(neighbors);

        all.ForEach(x => {
            if (Chunk.ComputeKey(x + Chunk.Offset) == Chunk.Key)
                UpdateCacheAt(x);
        });

        UpdateContiguousCache();
    }


    

    /// <summary>
    /// Reads the contiguous arrays and writes it to buffers. 
    /// </summary>
    private void SendToGpu() {
        if (BuffersUpToDate)
            return;

        //if the buffer is not up to date, but the buffer is filled with something, then it's fine.
        //we may miss a frame of update, but not critical
        if (!ContiguousCacheUpToDate && IndexBuffer.Length > 0)
            return;

        while (!ContiguousCacheUpToDate)
            Thread.Sleep(1);

        VertexBuffer.Fill(ContiguousVerticesCache);
        IndexBuffer.Fill(ContiguousIndicesCache);

        BuffersUpToDate = true;
    }

    /// <summary>
    /// Assembles the contiguous arrays from the Cache. This is "quite" a costly operation.
    /// </summary>
    private void UpdateContiguousCache() {
        List<Vertex> vertices = new();
        List<uint> indices = new();

        // add the count of already processed vertices, so the indices refer to the correct vertex
        uint vertexCount = 0;
        foreach (var renderStuff in Cache.Values) {
            vertices.AddRange(renderStuff.Item1);
            indices.AddRange(renderStuff.Item2.Select(x => x + vertexCount));

            vertexCount += (uint)renderStuff.Item1.Length;
        }

        ContiguousVerticesCache = vertices.ToArray();
        ContiguousIndicesCache = indices.ToArray();
        ContiguousCacheUpToDate = true;
    }


    /// <summary>
    /// Binds all buffers automatically and renders this chunk. The texture stack and the corresponding program need to be bound though.
    /// </summary>
    public void Render() {
        SendToGpu();
        VertexBuffer.Bind();
        Vertex.UseVAO();
        IndexBuffer.Bind();
        GlLogger.WriteGLError();

        GL.DrawElements(PrimitiveType.Triangles, IndexBuffer.Length, DrawElementsType.UnsignedInt, 0);
    }

    public void Dispose() {
        VertexBuffer.Dispose();
        IndexBuffer.Dispose();
    }
}