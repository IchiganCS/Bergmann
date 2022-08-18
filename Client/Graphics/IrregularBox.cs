using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics;

public sealed class IrregularBox {

    /// <summary>
    /// All of these planes point outward, so the point lies on the left.
    /// </summary>
    private NormalPlane[] Planes { get; set; }

    public IrregularBox(Matrix4 projection, Matrix4 view) {
        Matrix4 inverseProj = projection.Inverted();
        Matrix4 inverseView = view.Inverted();

        Vector4 temp_bottomBackLeft = new Vector4(-1, -1, -1, 1) * inverseProj;
        Vector4 temp_bottomBackRight = new Vector4(1, -1, -1, 1) * inverseProj;
        Vector4 temp_bottomFrontLeft = new Vector4(-1, -1, 1, 1) * inverseProj;
        Vector4 temp_bottomFrontRight = new Vector4(1, -1, 1, 1) * inverseProj;
        Vector4 temp_topBackLeft = new Vector4(-1, 1, -1, 1) * inverseProj;
        Vector4 temp_topBackRight = new Vector4(1, 1, -1, 1) * inverseProj;
        Vector4 temp_topFrontLeft = new Vector4(-1, 1, 1, 1) * inverseProj;
        Vector4 temp_topFrontRight = new Vector4(1, 1, 1, 1) * inverseProj;

        // TODO: make this use Vector3.Unproject maybe?
        Vector3 bottomBackLeft = (temp_bottomBackLeft / temp_bottomBackLeft.W * inverseView).Xyz;
        Vector3 bottomBackRight = (temp_bottomBackRight / temp_bottomBackRight.W * inverseView).Xyz;
        Vector3 bottomFrontLeft = (temp_bottomFrontLeft / temp_bottomFrontLeft.W * inverseView).Xyz;
        Vector3 bottomFrontRight = (temp_bottomFrontRight / temp_bottomFrontRight.W * inverseView).Xyz;
        Vector3 topBackLeft = (temp_topBackLeft / temp_topBackLeft.W * inverseView).Xyz;
        Vector3 topBackRight = (temp_topBackRight / temp_topBackRight.W * inverseView).Xyz;
        Vector3 topFrontLeft = (temp_topFrontLeft / temp_topFrontLeft.W * inverseView).Xyz;
        Vector3 topFrontRight = (temp_topFrontRight / temp_topFrontRight.W * inverseView).Xyz;


        // Vector3 bottomBackLeft = ((temp_bottomBackLeft / temp_bottomBackLeft.W + (-margin, -margin, -margin, 0)) * inverseView).Xyz;
        // Vector3 bottomBackRight = ((temp_bottomBackRight / temp_bottomBackRight.W + (margin, -margin, -margin, 0)) * inverseView).Xyz;
        // Vector3 bottomFrontLeft = ((temp_bottomFrontLeft / temp_bottomFrontLeft.W + (-margin, -margin, margin, 0)) * inverseView).Xyz;
        // Vector3 bottomFrontRight = ((temp_bottomFrontRight / temp_bottomFrontRight.W + (margin, -margin, margin, 0)) * inverseView).Xyz;
        // Vector3 topBackLeft = ((temp_topBackLeft / temp_topBackLeft.W + (-margin, margin, -margin, 0)) * inverseView).Xyz;
        // Vector3 topBackRight = ((temp_topBackRight / temp_topBackRight.W + (margin, margin, -margin, 0)) * inverseView).Xyz;
        // Vector3 topFrontLeft = ((temp_topFrontLeft / temp_topFrontLeft.W + (-margin, margin, margin, 0)) * inverseView).Xyz;
        // Vector3 topFrontRight = ((temp_topFrontRight / temp_topFrontRight.W + (margin, margin, margin, 0)) * inverseView).Xyz;

        float margin = (float)Math.Sqrt(8 * 8 + 8 * 8 + 8 * 8);
        // Quaternion viewRot = inverseView.ExtractRotation();
        // Vector3 rotPoint = viewRot * new Vector3(margin, margin, margin);
        // margin = rotPoint.X;

        // bottomBackLeft += (-margin, -margin, -margin);
        // bottomBackRight += (margin, -margin, -margin);
        // bottomFrontLeft += (-margin, -margin, margin);
        // bottomFrontRight += (margin, -margin, margin);
        // topBackLeft += (-margin, margin, -margin);
        // topBackRight += (margin, margin, -margin);
        // topFrontLeft += (-margin, margin, margin);
        // topFrontRight += (margin, margin, margin);

        Planes = new NormalPlane[6];

        //all of those planes point outward

        Planes[0] = new( //bottom
            Vector3.Cross(bottomFrontLeft - bottomFrontRight, bottomBackRight - bottomFrontRight).Normalized(),
            bottomFrontRight
        );
        Planes[1] = new( //right
            Vector3.Cross(topBackRight - bottomBackRight, bottomFrontRight - bottomBackRight).Normalized(),
            bottomBackRight
        );
        Planes[2] = new( //left
            Vector3.Cross(topFrontLeft - bottomFrontLeft, bottomBackLeft - bottomFrontLeft).Normalized(),
            bottomFrontLeft
        );
        Planes[3] = new( //front
            Vector3.Cross(topFrontRight - bottomFrontRight, bottomFrontLeft - bottomFrontRight).Normalized(),
            bottomFrontRight
        );
        Planes[4] = new( //back
            Vector3.Cross(bottomBackLeft - bottomBackRight, topBackRight - bottomBackRight).Normalized(),
            bottomBackRight
        );
        Planes[5] = new( //top
            Vector3.Cross(topFrontRight - topFrontLeft, topBackLeft - topFrontLeft).Normalized(),
            topFrontLeft
        );
    }

    public bool Contains(Vector3 point) {
        return Planes.All(x => !x.IsOnRight(point));
    }


    private sealed class NormalPlane {
        /// <summary>
        /// Normal should point to the right of the plane.
        /// </summary>
        public Vector3 Normal { get; set; }

        public Vector3 Point { get; set; }

        public NormalPlane(Vector3 normal, Vector3 point) {
            Normal = normal;
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