using OpenTK.Mathematics;

namespace Bergmann.Shared.Objects;


/// <summary>
/// Holds methods to do geometrical operations on objects in the world. Also defines some extension methods if they are so
/// geometrical in nature that the decoupling can be reasonably executed.
/// </summary>
public static class Geometry {
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
    /// Gets all neighboring blocks from a given position using an iteration over <see cref="FaceToVector"/>
    /// </summary>
    /// <param name="position">The origin position</param>
    /// <returns>All neighboring blocks; the origin is not included</returns>
    public static IEnumerable<Vector3i> AllNeighbors(Vector3i position) {
        return Enumerable.Range(0, 6).Select(x => position + FaceToVector[x]);
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
    /// Gets a list of chunks near the origin in a given distance. The distance is only measured horizontally, not vertically.
    /// That means, chunk columns starting at y=0 are returned. No chunks above though.
    /// The result should be cached, since this can't be made a fast operation.
    /// </summary>
    /// <param name="distance">The distance in world chunk space.</param>
    /// <returns>A list of offsets in world space.</returns>
    public static IEnumerable<Vector3i> GetNearChunkColumns(int distance) {

        // because the result is a sphere, we start by first calculating the positive part of the result.
        // The rest of the vectors are calculated by negating single components.
        List<Vector3i> positiveOffsets = new();


        for (int x = 0; x < distance; x++) {
            for (int z = 0; z < distance; z++) {
                Vector3i vec = (x, 0, z);
                if (vec.ManhattanLength <= distance) {
                    positiveOffsets.Add(vec * 16);
                }
            }
        }

        //don't include negative offsets
        return positiveOffsets.SelectMany(pos => new Vector3i[] {
            pos * (1, 1, 1),
            pos * (-1, 1, 1),
            //pos * (1, -1, 1),
            pos * (1, 1, -1),
            //pos * (1, -1, -1),
            pos * (-1, 1, -1),
            //pos * (-1, -1, 1),
            //pos * (-1, -1, -1),
        }).Distinct();
    }



    /// <summary>
    /// Cast a ray from origin in the direction of destination. Returns whether there has been a hit and if that value 
    /// is true, the hit face and position of that block is returned. Since this logically needs to be distance
    /// limited, the limit is <paramref name="distance"/>.
    /// </summary>
    /// <param name="origin">The origin of the ray. If origin lies in a block, that same block is returned</param>
    /// <param name="direction">The direction shot from origin</param>
    /// <param name="distance">The distance when to end the raycast.</param>
    /// <param name="hitBlock">The position of the block hit by the raycast.</param>
    /// <param name="hitFace">The hit face of the block.</param>
    /// <returns>Whether there was a hit in <paramref name="distance"/>.</returns>
    public static bool Raycast(this ChunkCollection collection,
        Vector3 origin, Vector3 direction, out Vector3i hitBlock, out Geometry.Face hitFace, float distance = 5) {


        //this method works like this:
        //We use the GetFaceFromHit method to walk through each face that lies along direction.
        //We truly walk along every block - quite elegant.

        Vector3 position = origin;

        //but because we could get stuck on exactly an edge and possibly hit the same cube over and over again
        //we have to add a slight delta to move into the direction after each block move
        Vector3 directionDelta = direction;
        directionDelta.NormalizeFast();
        directionDelta /= 100f;

        int i = (int)distance * 10;
        while ((position - origin).LengthSquared < distance * distance) {
            i--;

            Vector3i flooredPosition = new(
                (int)Math.Floor(position.X),
                (int)Math.Floor(position.Y),
                (int)Math.Floor(position.Z));

            Block current = collection.GetBlockAt(flooredPosition);
            if (current > 0) {
                hitBlock = flooredPosition;
                hitFace = Geometry.GetFaceFromHit(position - flooredPosition, direction, out _);
                return true;
            }

            _ = Geometry.GetFaceFromHit(position - flooredPosition, -direction, out Vector3 hit);

            if (i <= 0) {
                Logger.Warn("Something went wrong, returning no hit");
                break;
            }

            position = hit + flooredPosition + directionDelta;
        }

        hitFace = Geometry.Face.Front;
        hitBlock = (0, 0, 0);
        return false;
    }
}