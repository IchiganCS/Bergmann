
using Bergmann.Shared.World;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.OpenGL.Renderers;

public class ChunkRenderer : IDisposable {
    private Chunk Chunk { get; set; }
    private Buffer VertexBuffer { get; set; }
    private Buffer IndexBuffer { get; set; }

    /// <summary>
    /// The key is the block position given by x * 16 * 16 + y * 16 + z. The pair stores each rendered vertex of the block
    /// with the appropriate properties, the uint array stores the indices for these vertices. These indices are local for their key, there are many doubles overall
    /// </summary>
    private Dictionary<int, (Vertex[], uint[])> Cache { get; set; }

#pragma warning disable CS8618
    public ChunkRenderer(Chunk chunk) {
        Chunk = chunk;

        Chunk.OnUpdate += UpdateBuffers;

        BuildCache();
        SendToGpu();
    }
#pragma warning restore CS8618

    private void BuildCache() {

        Cache = new();

        List<Vector3i> blocks = Chunk.EveryBlock();

        foreach (Vector3i block in blocks) {
            foreach (Block.Face face in Block.AllFaces) {
                int key = block.X * 16 * 16 + block.Y * 16 + block.Z;

                if (!Chunk.HasNeighborAt(block, face)) {
                    //we have to add it to the dicitionary

                    Vector3[] ps = Block.Positions[(int)face];
                    Vertex[] positions = new Vertex[4] {
                        new() { Position = ps[0] + block + Chunk.Offset, TexCoord = new(1, 0) },
                        new() { Position = ps[1] + block + Chunk.Offset, TexCoord = new(1, 1) },
                        new() { Position = ps[2] + block + Chunk.Offset, TexCoord = new(0, 1) },
                        new() { Position = ps[3] + block + Chunk.Offset, TexCoord = new(0, 0) }
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
    }

    private void SendToGpu() {
        if (VertexBuffer is not null)
            VertexBuffer.Dispose();
        if (IndexBuffer is not null)
            IndexBuffer.Dispose();


        VertexBuffer = new Buffer(BufferTarget.ArrayBuffer);
        IndexBuffer = new Buffer(BufferTarget.ElementArrayBuffer);

        List<Vertex> vertices = new();
        List<uint> indices = new();

        // add the count of already processed vertices, so the indices do refer to the correct vertex
        uint vertexCount = 0;
        foreach (var renderStuff in Cache.Values) {
            vertices.AddRange(renderStuff.Item1);
            indices.AddRange(renderStuff.Item2.Select(x => x + vertexCount));

            vertexCount += (uint)renderStuff.Item1.Length;
        }

        VertexBuffer.Fill(vertices.ToArray());
        IndexBuffer.Fill(indices.ToArray());
    }


    public void Render() {

        VertexBuffer.Bind();
        Vertex.UseVAO();
        IndexBuffer.Bind();
        GlLogger.WriteGLError();

        GL.DrawElements(PrimitiveType.Triangles, IndexBuffer.Length, DrawElementsType.UnsignedInt, 0);
    }

    private void UpdateBuffers(List<Vector3i> positions) {
        //TODO

        SendToGpu();
    }
    
    public void Dispose() {
        VertexBuffer.Dispose();
        IndexBuffer.Dispose();
    }
}