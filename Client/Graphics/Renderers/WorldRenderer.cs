using Bergmann.Client.Graphics.Renderers.Passers;

namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// A renderer for the entire world. Of course, it doesn't render the entire world, but only handles
/// the different render passes. It holds a set of <see cref="IRendererPasser"/> to achieve this.
/// </summary>
public class WorldRenderer : IDisposable, IRenderer {

    private IList<IRendererPasser> RenderPassers { get; set; }


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