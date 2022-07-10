using OpenTK.Graphics.OpenGL;

namespace Bergmann.Client.Graphics.OpenGL;

public class Texture : IDisposable {
    public int Handle { get; set; }

    public Texture() {
        Handle = GL.GenTexture();
    }

    

    public void Dispose() {
        GL.DeleteTexture(Handle);
        Handle = 0;
    }
}