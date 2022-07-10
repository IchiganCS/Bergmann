using OpenTK.Mathematics;

namespace Bergmann.Shared.World;

/// <summary>
/// Block is effectively an integer stored in <see cref="Block.Type"/>. Implicit conversions exist.
/// All information for type can be statically retrieved by methods, e.g. texture coordinates and so on.
/// This struct also defines a lot of static methods/members for working geometrically with blocks
/// </summary>
public struct Block {
    #region Statics
    public enum Face {
        Front, Bottom, Top, Back, Right, Left
    }
    public static readonly Face[] AllFaces = new Face[6] {
        Face.Front, Face.Bottom, Face.Top, Face.Back, Face.Right, Face.Left
    };
    public static readonly Vector3i[] FaceToVector = new Vector3i[6] {
        -Vector3i.UnitZ, -Vector3i.UnitY, Vector3i.UnitY, Vector3i.UnitZ, Vector3i.UnitX, -Vector3i.UnitX
    };

    /// <summary>
    /// Gets the face of the block where position lies in the block and is hit from <see cref="incomingDirection"/>.
    /// Position is treated to be in the base block at (0, 0, 0).
    /// </summary>
    /// <param name="position">A position in the block</param>
    /// <param name="incomingDirection">The direction from which a ray is cast.</param>
    /// <param name="hitPoint">The projected position through which incoming direction passes on the block: 
    /// It is as such guaranteed that at least one coordinate is either 1 or 0 
    /// to be on the side of the block.</param>
    /// <returns></returns>
    public static Face GetFaceFromHit(Vector3 position, Vector3 incomingDirection, out Vector3 hitPoint) {
        //calculate how long it would take for each face to be hit.
        //for that, determine how much space we need to travel in a direction
        //then divide that space through the velocity
        //the smallest time will be hit
        float spaceX, spaceY, spaceZ;
        float timeX, timeY, timeZ;
        if (incomingDirection.X < 0)
            spaceX = 1 - position.X;
        else
            spaceX = position.X;

        timeX = spaceX / Math.Abs(incomingDirection.X);

        if (incomingDirection.Y < 0)
            spaceY = 1 - position.Y;
        else
            spaceY = position.Y;

        timeY = spaceY / Math.Abs(incomingDirection.Y);

        if (incomingDirection.Z < 0)
            spaceZ = 1 - position.Z;
        else
            spaceZ = position.Z;

        timeZ = spaceZ / Math.Abs(incomingDirection.Z);

        if (timeX < timeY) {
            if (timeX < timeZ) {
                hitPoint = position + (-incomingDirection * timeX);
                if (incomingDirection.X > 0)
                    return Face.Left;
                else
                    return Face.Right;
            }
            else {
                hitPoint = position + (-incomingDirection * timeZ);
                if (incomingDirection.Z > 0)
                    return Face.Front;
                else
                    return Face.Back;
            }
        }
        else {
            if (timeY < timeZ) {
                hitPoint = position + (-incomingDirection * timeY);
                if (incomingDirection.Y > 0)
                    return Face.Bottom;
                else
                    return Face.Top;
            }
            else {
                hitPoint = position + (-incomingDirection * timeZ);
                if (incomingDirection.Z > 0)
                    return Face.Front;
                else
                    return Face.Back;
            }
        }
    }

    /// <summary>
    /// Apply the transformation to <see cref="FrontPositions"/> given by <see cref="Face"/> to get the transformed side.
    // /// </summary>
    private static readonly Matrix4[] Transformations = new Matrix4[6] {
        Matrix4.Identity, //front
        Matrix4.CreateTranslation(-1, 0, 0) * Matrix4.CreateRotationY(MathF.PI) * Matrix4.CreateRotationX(MathF.PI / 2), //bottom
        Matrix4.CreateRotationX(MathF.PI / 2) * Matrix4.CreateTranslation(0, 1, 0), //top
        Matrix4.CreateTranslation(-1, 0, -1) * Matrix4.CreateRotationY(MathF.PI), //back
        Matrix4.CreateTranslation(-1, 0, 0) * Matrix4.CreateRotationY(-MathF.PI / 2) * Matrix4.CreateTranslation(1, 0, 1), //right
        Matrix4.CreateTranslation(-1, 0, 0) * Matrix4.CreateRotationY(MathF.PI / 2) //left
    };
    
    /// <summary>
    /// The face for the front at the world origin
    /// </summary>
    private static readonly Vector4[] FrontPositions = new Vector4[4] {
        new(1, 0, 0, 1),
        new(1, 1, 0, 1),
        new(0, 1, 0, 1),
        new(0, 0, 0, 1)
    };
    /// <summary>
    /// The indices which can be used together with Positions
    /// </summary>
    public static readonly uint[] Indices = new uint[6] {
        0, 1, 2,
        0, 2, 3
    };
    /// <summary>
    /// Give it the face and it will give you the positions for face of the (0,0,0) cube
    /// </summary>
    public static readonly Vector3[][] Positions = new Vector3[6][] {
        new Vector3[] {
            (FrontPositions[0] * Transformations[0]).Xyz,
            (FrontPositions[1] * Transformations[0]).Xyz,
            (FrontPositions[2] * Transformations[0]).Xyz,
            (FrontPositions[3] * Transformations[0]).Xyz,
        },
        new Vector3[] {
            (FrontPositions[0] * Transformations[1]).Xyz,
            (FrontPositions[1] * Transformations[1]).Xyz,
            (FrontPositions[2] * Transformations[1]).Xyz,
            (FrontPositions[3] * Transformations[1]).Xyz,
        },
        new Vector3[] {
            (FrontPositions[0] * Transformations[2]).Xyz,
            (FrontPositions[1] * Transformations[2]).Xyz,
            (FrontPositions[2] * Transformations[2]).Xyz,
            (FrontPositions[3] * Transformations[2]).Xyz,
        },
        new Vector3[] {
            (FrontPositions[0] * Transformations[3]).Xyz,
            (FrontPositions[1] * Transformations[3]).Xyz,
            (FrontPositions[2] * Transformations[3]).Xyz,
            (FrontPositions[3] * Transformations[3]).Xyz,
        },
        new Vector3[] {
            (FrontPositions[0] * Transformations[4]).Xyz,
            (FrontPositions[1] * Transformations[4]).Xyz,
            (FrontPositions[2] * Transformations[4]).Xyz,
            (FrontPositions[3] * Transformations[4]).Xyz,
        },
        new Vector3[] {
            (FrontPositions[0] * Transformations[5]).Xyz,
            (FrontPositions[1] * Transformations[5]).Xyz,
            (FrontPositions[2] * Transformations[5]).Xyz,
            (FrontPositions[3] * Transformations[5]).Xyz,
        },
    };

    #endregion Statics

    public int Type { get; set; }
    public Block(int type) {
        Type = type;
    }

    public static implicit operator int(Block block)
        => block.Type;
    public static implicit operator Block(int type)
        => new(type);
}