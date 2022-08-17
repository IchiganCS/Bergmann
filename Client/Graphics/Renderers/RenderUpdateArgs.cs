namespace Bergmann.Client.Graphics.Renderers;

public class RenderUpdateArgs {
    public float DeltaTime { get; private set; }

    public RenderUpdateArgs(float deltaTime) {
        this.DeltaTime = deltaTime;
    }
}