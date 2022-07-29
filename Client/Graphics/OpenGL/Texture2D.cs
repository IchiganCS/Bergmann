using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Bergmann.Client.Graphics.OpenGL;

public class Texture2D : TextureBase {
    public int Width { get; private set; }
    public int Height { get; private set; }

    /// <summary>
    /// Creates a new texture with sensible default parameters.
    /// </summary>
    public Texture2D() : base(TextureTarget.Texture2D) {

    }


    public void Write(Image<Rgba32> image) {
        if (Width > 0 && Height > 0 && (image.Width != Width || image.Height != Height))
            Logger.Warn("Supplied image doesn't fit the previously supplied dimensions");


        GL.BindTexture(TextureTarget.Texture2D, Handle);
        image.Mutate(x => x.Flip(FlipMode.Vertical));
        byte[] pixels = new byte[4 * image.Width * image.Height];
        image.CopyPixelDataTo(pixels);
        GlLogger.WriteGLError();

        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba,
            image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            
        GlLogger.WriteGLError();

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        GlLogger.WriteGLError();
    }
}