using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;

namespace Bergmann.Client.Graphics.OpenGL;

public abstract class TextureBase : SafeGlHandle {

    public TextureTarget Target { get; private set; }

    public TextureBase(TextureTarget target) {
        HandleValue = GL.GenTexture();
        Target = target;


        GL.BindTexture(Target, (int)handle);
        GL.TexParameter(Target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(Target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
        GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
    }

    public void Bind() {
        if (IsClosed || IsInvalid) {
            Logger.Warn("Tried to bind invalid texture");
            return;
        }

        switch (Target) {
            case TextureTarget.Texture2D:
                GL.ActiveTexture(TextureUnit.Texture1);
                break;
            case TextureTarget.Texture2DArray:
                GL.ActiveTexture(TextureUnit.Texture0);
                break;
        }
        GL.BindTexture(Target, HandleValue);
    }

    protected override bool ReleaseHandle() {
        GlThread.Invoke(() => GL.DeleteTexture(HandleValue));        
        return true;
    }
}