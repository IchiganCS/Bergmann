namespace Bergmann.Client.Graphics.Renderers.Passers;

/// <summary>
/// An interface whose sub classes must provide a way to render itself
/// and adding and removing new chunks to the passer.
/// </summary>
public interface IRendererPasser : IDisposable {

    /// <summary>
    /// This render passer should add a new chunk. It is ensured that, at the time point of the calling,
    /// all surrounding chunks are loaded. Currently, this also is a kind of "this chunk has changed" method.
    /// </summary>
    /// <param name="key">The key to the chunk that should be rendered soon.</param>
    public void AddChunk(long key);

    /// <summary>
    /// The render passer should drop the currently rendered chunk.
    /// </summary>
    /// <param name="key">The key of the chunk not to be rendered later.</param>
    public void DropChunk(long key);


    /// <summary>
    /// Make an entire render passer. The supplied arguments should be used to make it faster.
    /// </summary>
    /// <param name="box">A frustum in world space. If the middle point of the chunk
    /// is not contained in this, the chunk will not be visible, therefore don't render it.</param>
    public void Render(Frustum box);
}