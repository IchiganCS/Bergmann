using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bergmann.Client.InputHandlers;

/// <summary>
/// Collects all functionality to handle first person cameras.
/// </summary>
public class FPSController {
    public static float FlySideSpeed { get; set; } = 4;
    public static float FlyForwardsSpeed { get; set; } = 4;
    public static float FlyBackwardSpeed { get; set; } = 4;
    public static float FlyUpSpeed { get; set; } = 4;
    public static float FlyDownSpeed { get; set; } = 4;

    /// <summary>
    /// Clamps the angle of the x rotation to this angle. Given in radians
    /// </summary>
    public static float ClampAngle { get; set; } = 85f / 180f * (float)Math.PI;
    /// <summary>
    /// The sensitvity of the mouse
    /// </summary>
    public static Vector2 Sensitivity { get; set; } = new(0.003f, 0.0028f);





    public Vector3 Position { get; set; }
    public Vector2 EulerAngles { get; set; }
    public Quaternion Rotation
        => Quaternion.FromEulerAngles(0, EulerAngles.Y, 0) *
            Quaternion.FromEulerAngles(EulerAngles.X, 0, 0);


    public Vector3 Forward
        => Rotation * new Vector3(0, 0, 1);

    /// <summary>
    /// Calculates the rotation of an fps camera with the given parameters
    /// </summary>
    /// <param name="mouseMovement">The movement of the mouse</param>
    public void RotateCamera(Vector2 mouseMovement) {
        var (x, y) = EulerAngles + (Sensitivity * mouseMovement).Yx;
        x = Math.Clamp(x, -ClampAngle, ClampAngle);

        if (Math.Abs(y) > 3.142)
            y -= (float)Math.CopySign(2 * Math.PI, y);

        EulerAngles = new(x, y);
    }

    /// <summary>
    /// Calculates where to fly with the given movement
    /// </summary>
    /// <param name="deltaTime">The time passed in the last frame</param>
    /// <param name="keyboard">The state of the keyboard. Value are retrieved from <see cref="KeyMappings"/> </param>
    public void FlyingMovement(float deltaTime, KeyboardState keyboard){

        float x = 0, y = 0, z = 0;
        if (keyboard.IsKeyDown(KeyMappings.Forward))
            z += FlyForwardsSpeed;
        if (keyboard.IsKeyDown(KeyMappings.Backwards))
            z -= FlyBackwardSpeed;

        if (keyboard.IsKeyDown(KeyMappings.Up))
            y += FlyUpSpeed;
        if (keyboard.IsKeyDown(KeyMappings.Down))
            y -= FlyDownSpeed;

        if (keyboard.IsKeyDown(KeyMappings.Right))
            x += FlySideSpeed;
        if (keyboard.IsKeyDown(KeyMappings.Left))
            x -= FlySideSpeed;



        Vector3 toRotate = new(x, 0, z);
        Vector3 result = Rotation * toRotate;
        result += new Vector3(0, y, 0);

        Position += result * deltaTime;
    }


    /// <summary>
    /// Constructs a look at matrix for this camera.
    /// </summary>
    public Matrix4 LookAt()
        => Matrix4.LookAt(Position, Position + Forward, new(0, 1, 0));

}