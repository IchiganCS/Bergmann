using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Bergmann.Client.Graphics.OpenGL;

public class Texture2D : IDisposable {
    public int Handle { get; set; }

    public bool IsWritten { get; private set; }

    public Texture2D() {
        Handle = GL.GenTexture();

        GL.BindTexture(TextureTarget.Texture2D, Handle);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        IsWritten = false;
    }

    /// <summary>
    /// Fills the texture
    /// </summary>
    /// <param name="image">Unaltered image by ImageSharp.</param>
    public void Write(Image<Rgba32> image) {
        if (IsWritten) {
            Logger.Warn("Tried to write to already finished texture");
            return;
        }
        
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        image.Mutate(x => x.Flip(FlipMode.Vertical));
        byte[] pixels = new byte[4 * image.Width * image.Height];
        image.CopyPixelDataTo(pixels);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        IsWritten = true;
    }

    public void Bind() {
        GL.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public void Dispose() {
        GL.DeleteTexture(Handle);
        Handle = 0;
    }
}