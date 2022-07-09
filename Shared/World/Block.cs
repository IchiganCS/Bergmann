using OpenTK.Mathematics;

namespace Bergmann.Shared.World;

public class Block {
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


}