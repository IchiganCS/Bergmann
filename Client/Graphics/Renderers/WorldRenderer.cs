using Bergmann.Shared.World;

namespace Bergmann.Client.Graphics.Renderers;

public class WorldRenderer : IDisposable {

    private Dictionary<int, ChunkRenderer> ChunkRenderers{ get; set; }

    public WorldRenderer() {
        ChunkRenderers = new();

        World.Instance.OnChunkLoading += NewChunkRenderer;
        
        foreach (Chunk ch in World.Instance.Chunks.Values)
            NewChunkRenderer(ch);
    }

    private void NewChunkRenderer(Chunk newChunk) {
        ChunkRenderer n = new(newChunk);

        if (ChunkRenderers.ContainsKey(newChunk.Key)) {
            ChunkRenderers[newChunk.Key].Dispose();
            ChunkRenderers[newChunk.Key] = n;
        } else {
            ChunkRenderers.Add(newChunk.Key, n);
        }
    }

    public void Render() {
        foreach (ChunkRenderer cr in ChunkRenderers.Values)
            cr?.Render();
    }


    public void Dispose() {
        foreach (ChunkRenderer cr in ChunkRenderers.Values)
            cr.Dispose();
    }
}