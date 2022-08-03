using OpenTK.Mathematics;

namespace Bergmann.Shared.Objects;

/// <summary>
/// Block is effectively an integer stored in <see cref="Block.Type"/>. Implicit conversions exist.
/// All information for type can be statically retrieved by methods, e.g. texture coordinates and so on.
/// </summary>
public struct Block {
    private const int INFO_MASK = 0b111111111111;



    public int Type { get; set; }

    public BlockInfo Info
        => BlockInfo.GetFromID(Type & INFO_MASK);

    public Block(int type) {
        Type = type;
    }

    public static implicit operator int(Block block)
        => block.Type;
    public static implicit operator Block(int type)
        => new(type);

}