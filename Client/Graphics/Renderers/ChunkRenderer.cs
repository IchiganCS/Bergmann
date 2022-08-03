using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Shared.Objects;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// Renders a chunk. This class is used extensively by <see cref="WorldRenderer"/>. It generally works by creating buffers on 
/// the gpu and achieving fast render calls. However, updates are therefore expensive, because the entire buffers 
/// need to be sent to the gpu whenever an update is called for. This class is further optimized to work 
/// with multiple threads. Check the docs to find out.
/// </summary>
public class ChunkRenderer : IRenderer {




    /// <summary>
    /// Saves the key of the rendered chunk.
    /// </summary>
    public long ChunkKey { get; private set; }




    /// <summary>
    /// This is a blocking operation, it may be executed on any thread. It waits for the lock on the arrays and fills them, 
    /// then queues an update on the gl thread. This gl thread then releases the <see cref="_Lock"/>. 
    /// Thus, only one update per chunk is possible per frame.
    /// </summary>
    /// <param name="chunk">The chunk to be used for updating. It is used to fill appropriate buffers.</param>
    public void Update(Chunk chunk) {
        
    }

    /// <summary>
    /// This object constructs a new chunk renderer. It is not yet usable, call <see cref="Update"/> on it first.
    /// </summary>
    public ChunkRenderer() {
    }


    /// <summary>
    /// Binds all buffers and renders this chunk. The texture stack and the corresponding 
    /// program need to be bound though. This method only will render something if an update had been queued before.
    /// </summary>
    public void Render() {
    }

    /// <summary>
    /// Disposes the buffers by the chunk.
    /// </summary>
    public void Dispose() {
    }
}