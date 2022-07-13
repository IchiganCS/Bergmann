using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Bergmann.Client.Graphics.OpenGL;

public class Texture : IDisposable {
    public int Handle { get; set; }

    public bool IsWritten { get; private set; }

    public TextureTarget Target { get; private set; }

    public Texture(TextureTarget target) {
        Handle = GL.GenTexture();

        Target = target;

        GL.BindTexture(Target, Handle);
        GL.TexParameter(Target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(Target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
        GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        IsWritten = false;
    }

    /// <summary>
    /// Fills the texture
    /// </summary>
    /// <param name="image">Unaltered image by ImageSharp.</param>
    /// <param name="level">The level if the texture is 3d or an array of 2d images. Can be ignored for 2d textures</param>
    public void Write(Image<Rgba32> image, int level = 0) {
        if (IsWritten) {
            Logger.Warn("Tried to write to already finished texture");
            return;
        }
        
        GL.BindTexture(TextureTarget.Texture2D, Handle);
        image.Mutate(x => x.Flip(FlipMode.Vertical));
        byte[] pixels = new byte[4 * image.Width * image.Height];
        image.CopyPixelDataTo(pixels);
        if (Target == TextureTarget.Texture2D)
            GL.TexImage2D(Target, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        else if (Target == TextureTarget.Texture2DArray)
            GL.TexImage3D(Target, 0, PixelInternalFormat.Rgba, image.Width, image.Height, level, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
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