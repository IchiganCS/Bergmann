using OpenTK.Graphics.OpenGL;

namespace Bergmann.Client.Graphics.OpenGL;

public abstract class TextureBase : IDisposable {
    public int Handle { get; private set; }

    public TextureTarget Target { get; private set; }

    public TextureBase(TextureTarget target) {
        Handle = GL.GenTexture();
        Target = target;


        GL.BindTexture(Target, Handle);
        GL.TexParameter(Target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(Target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
        GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    }

    public void Bind() {
        switch (Target) {
            case TextureTarget.Texture2D:
                GL.ActiveTexture(TextureUnit.Texture1);
                break;
            case TextureTarget.Texture2DArray:
                GL.ActiveTexture(TextureUnit.Texture0);
                break;
        }
        GL.BindTexture(Target, Handle);
    }


    public void Dispose() {
        GL.DeleteTexture(Handle);
        Handle = 0;
    }    
}