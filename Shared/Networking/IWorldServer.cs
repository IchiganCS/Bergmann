using OpenTK.Mathematics;

namespace Bergmann.Shared.Networking;

public interface IWorldServer {
    public void RequestChunk(long key);

    public void DestroyBlock(Vector3 position, Vector3 direction);
}