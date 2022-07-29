using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Bergmann.Client.Graphics.OpenGL;


/// <summary>
/// Represents a stack of texture which can be bound to a `sampler2Darray` in shaders. It therefore internally is a Texture2DArray.
/// </summary>
public class TextureStack : TextureBase {

    /// <summary>
    /// The value whether image storage has been reserved and can be accessed.
    /// </summary>
    public bool IsReserved { get; private set; }

    /// <summary>
    /// The width of all textures. It can only be set once.
    /// </summary>
    public int Width { get; private set; }
    /// <summary>
    /// The height of all textures. It can only be set once.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// The depth of the stack, the layer count. It can only be set once.
    /// </summary>
    public int Depth{ get; private set; }

    /// <summary>
    /// Generates a new texture stack with sensible default parameters.
    /// </summary>
    public TextureStack() : base(TextureTarget.Texture2DArray) {
        IsReserved = false;
    }

    /// <summary>
    /// Fills the texture. If no image was reserved, the action will fail.
    /// </summary>
    /// <param name="image">Unaltered image by ImageSharp.</param>
    /// <param name="level">The level (index) of an array of 2d images.</param>
    public void Write(Image<Rgba32> image, int level) {
        if (Width > 0 && Height > 0 && (image.Width != Width || image.Height != Height)) {
            Logger.Warn("Supplied image doesn't fit the previously supplied dimensions");
            return;
        }
        if (!IsReserved) {
            Logger.Warn("Can't write to texture stack if no reservation is made");
            return;
        }
        if (level >= Depth || level < 0) {
            Logger.Warn($"Invalid level/depth {level}");
            return;
        }


        GL.BindTexture(TextureTarget.Texture2DArray, Handle);
        image.Mutate(x => x.Flip(FlipMode.Vertical));
        byte[] pixels = new byte[4 * image.Width * image.Height];
        image.CopyPixelDataTo(pixels);
        GlLogger.WriteGLError();

        GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, level, image.Width, image.Height, 1, 
            PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

        GlLogger.WriteGLError();

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2DArray);
        GlLogger.WriteGLError();
    }

    /// <summary>
    /// Reserves memory on the gpu. Necessary if you can't provide the entire texture in one write call, for example when using
    /// texture 2d arrays.
    /// </summary>
    /// <param name="width">The width of all images that should be stored</param>
    /// <param name="height">The height of all images that should be stored</param>
    /// <param name="depth">The depth of the texture, the number of layers for 2d arrays.</param>
    public void Reserve(int width, int height, int depth) {
        Width = width;
        Height = height;
        Depth = depth;


        GL.BindTexture(TextureTarget.Texture2DArray, Handle);
        GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, width, height, depth);

        GlLogger.WriteGLError();
        IsReserved = true;
    }
}