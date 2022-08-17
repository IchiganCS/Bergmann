namespace Bergmann.Client.Controllers;

public class RenderUpdateArgs {
    public float DeltaTime { get; private set; }

    public RenderUpdateArgs(float deltaTime) {
        this.DeltaTime = deltaTime;
    }
}