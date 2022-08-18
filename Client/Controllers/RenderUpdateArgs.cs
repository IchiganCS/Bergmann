using OpenTK.Mathematics;

namespace Bergmann.Client.Controllers;

/// <summary>
/// A collection of data used for rendering a controller.
/// </summary>
public class RenderUpdateArgs {

    /// <summary>
    /// The time since the last call of the update in seconds.
    /// </summary>
    public float DeltaTime { get; private set; }

    public RenderUpdateArgs(float deltaTime) {
        this.DeltaTime = deltaTime;
    }
}