using Bergmann.Shared.Networking;
using Bergmann.Shared.Objects;
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

    public float WalkSideSpeed { get; set; } = 5;
    public float WalkForwardsSpeed { get; set; } = 5;
    public float WalkBackwardSpeed { get; set; } = 3;
    public float WalkBoostMult { get; set; } = 1.6f;

    /// <summary>
    /// The minimum space to be left between the handler and potential blocks.
    /// </summary>
    /// <value></value>
    public float MinBlockSpace { get; set; } = 0.135f;

    /// <summary>
    /// This will be the height of the camera above the ground.
    /// </summary>
    public float Height { get; set; } = 1.7f;

    /// <summary>
    /// Where to check for collision except in the camera. All postions are understood relative to the camera position.
    /// </summary>
    public IList<Vector3> Colliders = new List<Vector3>() { new(0, 0, 0), new(0, -1.5f, 0) };

    /// <summary>
    /// Clamps the angle of the x rotation to this angle. Given in radians
    /// </summary>
    public float ClampAngle { get; set; } = 89f / 180f * (float)Math.PI;
    /// <summary>
    /// The sensitvity of the mouse
    /// </summary>
    public Vector2 Sensitivity { get; set; } = new(0.003f, 0.0028f);

    /// <summary>
    /// Only considered in the <see cref="WalkingMovement"/> method. It is decreased and then applied to the position,
    /// except when jumping of course.
    /// </summary>
    public float GravitationalPull { get; set; } = 0;

    public bool IsGrounded { get; set; } = false;


    /// <summary>
    /// The current position of the camera.
    /// </summary>
    public Vector3 Position { get; set; }


    /// <summary>
    /// The angles of rotation around the axes. The x component specifies the rotation around 
    /// the x axis, the y component around the y axis.
    /// </summary>
    public Vector2 EulerAngles { get; set; }

    /// <summary>
    /// The rotation constructed from the stored <see cref="EulerAngles"/>.
    /// </summary>
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


    public void WalkingMovement(float deltaTime, KeyboardState keyboard) {
        float x = 0, z = 0;
        if (keyboard.IsKeyDown(KeyMappings.Forward))
            z += WalkForwardsSpeed * (keyboard.IsKeyDown(KeyMappings.Boost) ? WalkBoostMult : 1);
        if (keyboard.IsKeyDown(KeyMappings.Backwards))
            z -= WalkBackwardSpeed;

        if (keyboard.IsKeyDown(KeyMappings.Right))
            x += WalkSideSpeed;
        if (keyboard.IsKeyDown(KeyMappings.Left))
            x -= WalkSideSpeed;

        if (keyboard.IsKeyDown(KeyMappings.Up) && IsGrounded) {
            IsGrounded = false;
            GravitationalPull = 6.5f;
        }

        Position += deltaTime * (Quaternion.FromEulerAngles(0, EulerAngles.Y, 0) * new Vector3(x, 0, z));

        if (!IsGrounded)
            GravitationalPull = Math.Max(-40, GravitationalPull - deltaTime * 20f);

        Position += (0, deltaTime * GravitationalPull, 0);
    }


    /// <summary>
    /// Moves the camera out of any blocks with a margin and makes sure one there is <paramref name="playerHeight"/>
    /// space below the camera.
    /// </summary>
    private void CollisionRespect(Vector3 prevPos) {
        Vector3 direction = Position - prevPos;

        if (direction.Y < 0)
            direction.Y = 0;

        if (direction != (0, 0, 0)) {
            foreach (Vector3 collider in Colliders) {
                if (Connection.Active!.Chunks.Raycast(prevPos + collider, direction, out _, out _, out var hit2, direction.LengthFast * 1.03f)) {
                    Position = prevPos;
                    direction = (0, 0, 0);
                }
            }
        }

        // check in y direction.
        if (!IsGrounded) {
            if (Connection.Active!.Chunks.Raycast(Position, (0, -1, 0), out _, out _, out var hit, Height * 0.93f)) {
                GravitationalPull = 0;
                Position = hit + (0, Height, 0);
                IsGrounded = true;
            }
            //up
            if (Connection.Active!.Chunks.Raycast(Position, (0, 1, 0), out _, out _, out hit, 0.1f)) {
                GravitationalPull = -GravitationalPull;
                Position = (Position.X, prevPos.Y, Position.Z);
            }
        }
        else if (!Connection.Active!.Chunks.Raycast(Position, (0, -1, 0), out _, out _, out var hit, Height * 1.03f)) {
            IsGrounded = false;
        }

        // check in x and z direction
        Vector3[] directionsToCheck = new[] {
            new Vector3(direction.X, 0, 0),
            new Vector3(0, 0, direction.Z),
        }
        .Where(x => x != (0, 0, 0))
        .Select(Vector3.NormalizeFast)
        .ToArray();

        foreach (Vector3 currentDir in directionsToCheck) {
            foreach (Vector3 collider in Colliders) {
                if (Connection.Active!.Chunks.Raycast(Position + collider, currentDir, out _, out _, out var hitPoint, MinBlockSpace * 1.1f)) {
                    Position = hitPoint - currentDir * MinBlockSpace - collider;
                }
            }
        }
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
    public void HandleInput(InputUpdateArgs args) {
        if (args.MouseState.IsButtonPressed(KeyMappings.BlockDestruction)) {
            Connection.Active?.ClientToServerAsync(new BlockDestructionMessage(Position, Forward));
        }

        if (args.MouseState.IsButtonPressed(KeyMappings.BlockPlacement)) {
            Connection.Active?.ClientToServerAsync(new BlockPlacementMessage(Position, Forward, 1));
        }

        RotateCamera(args.MouseState.Delta);
        Vector3 cachedPosition = Position;
        //FlyingMovement(args.DeltaTime, args.KeyboardState);
        WalkingMovement(args.DeltaTime, args.KeyboardState);
        CollisionRespect(cachedPosition);


    }
}