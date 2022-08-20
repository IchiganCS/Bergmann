using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics;

/// <summary>
/// A frustum in world space. It contains methods to do geometric operations on it.
/// </summary>
public class Frustum {

    /// <summary>
    /// All of these planes point outward, so the point lies on the left.
    /// </summary>
    private NormalPlane[] Planes { get; set; }

    /// <summary>
    /// Construct the frustum using a backwards operation on the projection and view matrix.
    /// </summary>
    /// <param name="inverseProj">The inverted projection matrix. It transforms from NDC to camera space.</param>
    /// <param name="inverseView">The inverted view matrix. It transforms from camera space.</param>
    /// <param name="margin">An optional margin added to each corner of the frustum.</param>
    public Frustum(Matrix4 inverseProj, Matrix4 inverseView, float margin = 0) {
        Vector4 temp_bottomBackLeft = new Vector4(-1, -1, 1, 1) * inverseProj;
        Vector4 temp_bottomBackRight = new Vector4(1, -1, 1, 1) * inverseProj;
        Vector4 temp_bottomFrontLeft = new Vector4(-1, -1, -1, 1) * inverseProj;
        Vector4 temp_bottomFrontRight = new Vector4(1, -1, -1, 1) * inverseProj;
        Vector4 temp_topBackLeft = new Vector4(-1, 1, 1, 1) * inverseProj;
        Vector4 temp_topBackRight = new Vector4(1, 1, 1, 1) * inverseProj;
        Vector4 temp_topFrontLeft = new Vector4(-1, 1, -1, 1) * inverseProj;
        Vector4 temp_topFrontRight = new Vector4(1, 1, -1, 1) * inverseProj;



        temp_bottomBackLeft /= temp_bottomBackLeft.W;
        temp_bottomBackRight /= temp_bottomBackRight.W;
        temp_bottomFrontLeft /= temp_bottomFrontLeft.W;
        temp_bottomFrontRight /= temp_bottomFrontRight.W;
        temp_topBackLeft /= temp_topBackLeft.W;
        temp_topBackRight /= temp_topBackRight.W;
        temp_topFrontLeft /= temp_topFrontLeft.W;
        temp_topFrontRight /= temp_topFrontRight.W;
        
        //idk why some of these values are negated???
        temp_bottomBackLeft += (margin, -margin, -margin, 0);
        temp_bottomBackRight += (-margin, -margin, -margin, 0);
        temp_bottomFrontLeft += (margin, -margin, margin, 0);
        temp_bottomFrontRight += (-margin, -margin, margin, 0);
        temp_topBackLeft += (margin, margin, -margin, 0);
        temp_topBackRight += (-margin, margin, -margin, 0);
        temp_topFrontLeft += (margin, margin, margin, 0);
        temp_topFrontRight += (-margin, margin, margin, 0);


        // TODO: make this use Vector3.Unproject maybe?
        Vector3 bottomBackLeft = (temp_bottomBackLeft * inverseView).Xyz;
        Vector3 bottomBackRight = (temp_bottomBackRight * inverseView).Xyz;
        Vector3 bottomFrontLeft = (temp_bottomFrontLeft * inverseView).Xyz;
        Vector3 bottomFrontRight = (temp_bottomFrontRight * inverseView).Xyz;
        Vector3 topBackLeft = (temp_topBackLeft * inverseView).Xyz;
        Vector3 topBackRight = (temp_topBackRight * inverseView).Xyz;
        Vector3 topFrontLeft = (temp_topFrontLeft * inverseView).Xyz;
        Vector3 topFrontRight = (temp_topFrontRight * inverseView).Xyz;

        Planes = new NormalPlane[6];

        //all of those planes point outward

        Planes[0] = new( //bottom
            -Vector3.Cross(bottomFrontLeft - bottomFrontRight, bottomBackRight - bottomFrontRight).Normalized(),
            bottomFrontRight
        );
        Planes[1] = new( //right
            -Vector3.Cross(topBackRight - bottomBackRight, bottomFrontRight - bottomBackRight).Normalized(),
            bottomBackRight
        );
        Planes[2] = new( //left
            -Vector3.Cross(topFrontLeft - bottomFrontLeft, bottomBackLeft - bottomFrontLeft).Normalized(),
            bottomFrontLeft
        );
        Planes[3] = new( //front
            -Vector3.Cross(topFrontRight - bottomFrontRight, bottomFrontLeft - bottomFrontRight).Normalized(),
            bottomFrontRight
        );
        Planes[4] = new( //back
            -Vector3.Cross(bottomBackLeft - bottomBackRight, topBackRight - bottomBackRight).Normalized(),
            bottomBackRight
        );
        Planes[5] = new( //top
            -Vector3.Cross(topFrontRight - topFrontLeft, topBackLeft - topFrontLeft).Normalized(),
            topFrontLeft
        );
    }

    /// <summary>
    /// Whether a given point lies in the frustum.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool Contains(Vector3 point) {
        return Planes.All(x => !x.IsOnRight(point));
    }


    /// <summary>
    /// A plane in 3d space identified by a normal. Just a helper for frustum.
    /// </summary>
    private sealed class NormalPlane {
        /// <summary>
        /// Normal should point to the right of the plane.
        /// </summary>
        public Vector3 Normal { get; set; }

        public Vector3 Point { get; set; }

        public NormalPlane(Vector3 rightNormal, Vector3 point) {
            Normal = rightNormal;
            Point = point;
        }

        /// <summary>
        /// If the point lies in the direction of the normal vector.
        /// </summary>
        /// <param name="point">The point to be checked.</param>
        /// <returns>On which side of the plane the given point lies.</returns>
        public bool IsOnRight(Vector3 point) {
            return Vector3.Dot(Normal, point - Point) > 0;
        }
    }
}