using Bergmann.Shared.World;

namespace Bergmann.Client.Graphics.OpenGL.Renderers;

public class WorldRenderer : IDisposable {
    private World World { get; set; }

    /// <summary>
    /// Holds a chunk renderer for each Chunk in the world. The place is (x * World.Distance + z) * y
    /// Can be replaced when the world loads a new chunk. Care for disposing.
    /// </summary>
    private ChunkRenderer[] ChunkRenderers { get; set; }

    public WorldRenderer(World world) {
        World = world;
        ChunkRenderers = new ChunkRenderer[World.Distance * World.Distance * World.ColumnHeight];

        World.OnChunkLoading += NewChunkRenderer;
    }

    private void NewChunkRenderer(Chunk newChunk, int x, int y, int z) {
        int pos = (x * World.Distance + z) * y;

        if (ChunkRenderers[pos] is not null)
            ChunkRenderers[pos]!.Dispose();

        ChunkRenderers[pos] = new(newChunk);
    }

    public void Render() {
        foreach (ChunkRenderer cr in ChunkRenderers)
            cr?.Render();
    }


    public void Dispose() {
        foreach (ChunkRenderer cr in ChunkRenderers)
            cr.Dispose();
    }
}