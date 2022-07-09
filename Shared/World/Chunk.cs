using OpenTK.Mathematics;

namespace Bergmann.Shared.World;

public class Chunk {
    public const int CHUNK_SIZE = 16;

    public int[,,] Blocks { get; set; }
    public Vector3i Offset { get; set; }

    public Chunk() {
        Blocks = new int[CHUNK_SIZE, CHUNK_SIZE, CHUNK_SIZE];
        for (int i = 0; i < Blocks.GetLength(0); i++)
            for (int j = 0; j < Blocks.GetLength(1); j++)
                for (int k = 0; k < Blocks.GetLength(2); k++)
                    Blocks[i, j, k] = 1;
    }

    public List<Vector3i> EveryBlock() {
        List<Vector3i> ls = new();
        for (int i = 0; i < Blocks.GetLength(0); i++)
            for (int j = 0; j < Blocks.GetLength(1); j++)
                for (int k = 0; k < Blocks.GetLength(2); k++)
                    if (Blocks[i, j, k] != 0)
                        ls.Add(new(i, j, k));

        return ls;
    }

    /// <summary>
    /// Checks whether the block has a neighbor
    /// </summary>
    /// <param name="position">The block whose neighbors are checked</param>
    /// <param name="direction">The direction relative to the block where there is a neighbor</param>
    /// <returns></returns>
    public bool HasNeighborAt(Vector3i position, Block.Face direction) {
        var (x, y, z) = position + Block.FaceToVector[(int)direction];

        //Chunk border
        if (x < 0 || x > CHUNK_SIZE - 1 ||
            y < 0 || y > CHUNK_SIZE - 1 ||
            z < 0 || z > CHUNK_SIZE - 1)
            return false; //TODO
            
        return Blocks[x, y, z] != 0;
    }

    /// <summary>
    /// The delegate which is called when this chunk updates
    /// </summary>
    /// <param name="positions">The positions of each block that has changed. Either through replacment, deletion or any other update</param>
    public delegate void UpdateDelegate(List<Vector3i> positions);
    /// <summary>
    /// Called whenever any block changes states or the blocks itself change
    /// </summary>
    public event UpdateDelegate OnUpdate;
}