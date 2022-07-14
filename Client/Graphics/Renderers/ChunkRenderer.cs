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
public class ChunkRenderer : IDisposable {
    private Chunk Chunk { get; set; }
    private OpenGL.Buffer VertexBuffer { get; set; }
    private OpenGL.Buffer IndexBuffer { get; set; }

    /// <summary>
    /// The key is the block position given by x * 16 * 16 + y * 16 + z. The pair stores each rendered vertex of the block
    /// with the appropriate properties, the uint array stores the indices for these vertices. These indices are local for their key, there are many doubles overall
    /// </summary>
    private Dictionary<int, (Vertex[], uint[])> Cache { get; set; }

    #pragma warning disable CS8618
    public ChunkRenderer(Chunk chunk) {
        Chunk = chunk;

        Chunk.OnUpdate += Update;

        BuildCache();
        SendToGpu();
    }
    #pragma warning restore CS8618

    /// <summary>
    /// Looks up each block and each face and loads the Cache. This is a very costly operation and should 
    /// preferabbly only executed once.
    /// </summary>
    private void BuildCache() {
        Cache = new();

        foreach (Vector3i block in  Chunk.EveryBlock()) {
            UpdateCacheAt(block);
        }
    }

    /// <summary>
    /// Updates the cache at a specific position.
    /// </summary>
    /// <param name="position">The position in chunk space</param>
    private void UpdateCacheAt(Vector3i block) {
        BlockInfo bi = ((Block)Chunk.Blocks[block.X, block.Y, block.Z]).Info;
        int key = block.X * 16 * 16 + block.Y * 16 + block.Z;
        Cache.Remove(key);

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
                    Cache.Add(key, (positions, Block.Indices));
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

        SendToGpu();
    }


    /// <summary>
    /// Assembles the contiguous arrays from the Cache and writes it to buffers. 
    /// This is "quite" a costly operation.
    /// </summary>
    private void SendToGpu() {
        if (VertexBuffer is not null)
            VertexBuffer.Dispose();
        if (IndexBuffer is not null)
            IndexBuffer.Dispose();


        VertexBuffer = new OpenGL.Buffer(BufferTarget.ArrayBuffer);
        IndexBuffer = new OpenGL.Buffer(BufferTarget.ElementArrayBuffer);

        List<Vertex> vertices = new();
        List<uint> indices = new();

        // add the count of already processed vertices, so the indices refer to the correct vertex
        uint vertexCount = 0;
        foreach (var renderStuff in Cache.Values) {
            vertices.AddRange(renderStuff.Item1);
            indices.AddRange(renderStuff.Item2.Select(x => x + vertexCount));

            vertexCount += (uint)renderStuff.Item1.Length;
        }

        VertexBuffer.Fill(vertices.ToArray());
        IndexBuffer.Fill(indices.ToArray());
    }

    
    /// <summary>
    /// Binds all buffers automatically and renders this chunk. The texture stack and the corresponding program need to be bound though.
    /// </summary>
    public void Render() {
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