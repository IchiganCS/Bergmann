using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Bergmann.Client.Graphics.OpenGL;

/// <summary>
/// Represents any 2d texture. It can be refilled for better performance.
/// </summary>
public class Texture2D : TextureBase {

    /// <summary>
    /// The width of the texture. It can only be set once.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// The height of the texture. It can only be set once.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Creates a new texture with sensible default parameters.
    /// </summary>
    public Texture2D() : base(TextureTarget.Texture2D) {

    }



    /// <summary>
    /// Writes an image to the texture. If this action is performed to overwrite an already exisisting texture,
    /// make sure the dimensions align. Otherwise, generate a new texture.
    /// </summary>
    /// <param name="image">The image to be written.</param>
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