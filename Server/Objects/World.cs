using Bergmann.Shared.Objects;

namespace Bergmann.Server.Objects;

public class World {

    public ChunkCollection Chunks { get; private set; }

    /// <summary>
    /// Constructs a new world. Initializes completely new instance.
    /// </summary>
    public World() {
        Chunks = new();
    }


    public void Clear() {
        Chunks = new();
    }
}