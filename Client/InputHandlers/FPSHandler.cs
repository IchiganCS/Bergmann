using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Bergmann.Client.InputHandlers;

/// <summary>
/// Collects all functionality to handle first person cameras.
/// </summary>
public class FPHandler : IInputHandler {
    public float FlySideSpeed { get; set; } = 4;
    public float FlyForwardsSpeed { get; set; } = 4;
    public float FlyBackwardSpeed { get; set; } = 4;
    public float FlyUpSpeed { get; set; } = 4;
    public float FlyDownSpeed { get; set; } = 4;
    public float FlyBoostMult { get; set; } = 10;

    /// <summary>
    /// Clamps the angle of the x rotation to this angle. Given in radians
    /// </summary>
    public float ClampAngle { get; set; } = 85f / 180f * (float)Math.PI;
    /// <summary>
    /// The sensitvity of the mouse
    /// </summary>
    public Vector2 Sensitivity { get; set; } = new(0.003f, 0.0028f);





    public Vector3 Position { get; set; }

    
    /// <summary>
    /// The angles of rotation around the axes. The x component specifies the rotation around 
    /// the x axis, the y component around the y axis.
    /// </summary>
    public Vector2 EulerAngles { get; set; }
    public Quaternion Rotation
        => Quaternion.FromEulerAngles(0, EulerAngles.Y, 0) *
            Quaternion.FromEulerAngles(EulerAngles.X, 0, 0);


    public Vector3 Forward
        => Rotation * new Vector3(0, 0, 1);

    /// <summary>
    /// Applies the rotation of an fps camera with the given parameters
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
    /// Calculates and applies where to fly with the given movement
    /// </summary>
    /// <param name="deltaTime">The time passed in the last frame</param>
    /// <param name="keyboard">The state of the keyboard. Values are retrieved from <see cref="KeyMappings"/>.</param>
    public void FlyingMovement(float deltaTime, KeyboardState keyboard) {

        float x = 0, y = 0, z = 0;
        if (keyboard.IsKeyDown(KeyMappings.Forward))
            z += FlyForwardsSpeed * (keyboard.IsKeyDown(KeyMappings.Boost) ? FlyBoostMult : 1);
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
    public Matrix4 LookAt
        => Matrix4.LookAt(Position, Position + Forward, new(0, 1, 0));


    /// <summary>
    /// Rotates the camera and moves it as specified by <paramref name="args"/>.
    /// </summary>
    /// <param name="args">The values used to update.</param>
    public void HandleInput(UpdateArgs args) {
        RotateCamera(args.MouseState.Delta);
        FlyingMovement(args.DeltaTime, args.KeyboardState);            
    }
}