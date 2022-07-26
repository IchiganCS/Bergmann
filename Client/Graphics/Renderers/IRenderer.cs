namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// A unified interface for all renderers. The added dispose method is helpful in some cases.
/// </summary>
public interface IRenderer : IDisposable {

    /// <summary>
    /// Renders the renderer
    /// </summary>
    public void Render();
}