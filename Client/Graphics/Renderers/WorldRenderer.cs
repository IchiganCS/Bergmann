using Bergmann.Client.Graphics.Renderers.Passers;

namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// A renderer for the entire world. Of course, it doesn't render the entire world, but only handles
/// the rendering of chunks. It holds a set of <see cref="ChunkRenderer"/> which are automatically created
/// and destroyed when called for. It registers to the events of the <see cref="World"/> class to achieve this.
/// </summary>
public class WorldRenderer : IDisposable, IRenderer {

    private IList<IRendererPasser> RenderPassers { get; set; }

    /// <summary>
    /// Constructs a world renderer for the <see cref="World.Instance"/>. It subscribes to updates from the world hub
    /// for chunk receiving and updating. It currently doesn't support loading chunks on startup, it is recommended to
    /// call this method before working on chunks.
    /// </summary>
    public WorldRenderer() {
        RenderPassers = new List<IRendererPasser>() {
            new SolidsPasser()
        };
    }


    public void Render() {
        foreach (IRendererPasser passer in RenderPassers)
            passer.Render();
    }


    public void Dispose() {
        foreach (IRendererPasser passer in RenderPassers)
            passer.Dispose();
    }
}