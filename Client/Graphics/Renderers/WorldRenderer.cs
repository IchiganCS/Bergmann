using Bergmann.Shared.World;

namespace Bergmann.Client.Graphics.OpenGL.Renderers;

public class WorldRenderer : IDisposable {
    private World World { get; set; }

    private Dictionary<int, ChunkRenderer> ChunkRenderers{ get; set; }

    public WorldRenderer(World world) {
        World = world;
        ChunkRenderers = new();

        World.OnChunkLoading += NewChunkRenderer;
    }

    private void NewChunkRenderer(Chunk newChunk) {
        if (ChunkRenderers.ContainsKey(newChunk.Key)) {
            ChunkRenderers[newChunk.Key].Dispose();
            ChunkRenderers[newChunk.Key] = new(newChunk);
        } else {
            ChunkRenderers.Add(newChunk.Key, new(newChunk));
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