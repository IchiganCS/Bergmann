using Bergmann.Shared;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Bergmann.Client.Graphics.OpenGL;

public class Texture : IDisposable {
    public int Handle { get; set; }

    /// <summary>
    /// The value whether image storage has been reserved and can be accessed.
    /// </summary>
    public bool IsReserved { get; private set; }

    public TextureTarget Target { get; private set; }

    public int Width { get; private set; }
    public int Height { get; private set; }

    /// <summary>
    /// Creates a new texture with sensible default parameters.
    /// </summary>
    /// <param name="target">The target of the texture. Only 2d and 2d array are currently supported</param>
    public Texture(TextureTarget target) {
        Handle = GL.GenTexture();

        Target = target;

        switch (target) {
            case TextureTarget.Texture2D:
            case TextureTarget.Texture2DArray:
                break;
            default:
                Logger.Warn($"Invalid type {target} for texture");
                break;
        }

        GL.BindTexture(Target, Handle);
        GL.TexParameter(Target, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TexParameter(Target, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.TexParameter(Target, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
        GL.TexParameter(Target, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);

        IsReserved = false;
    }

    /// <summary>
    /// Fills the texture. If no image was reserved, it acts as if it is the only texture ever to be written.
    /// If multiple calls to perform consecutive writes are to be expeceted, you have to <see cref="Reserve"/> the memory first.
    /// </summary>
    /// <param name="image">Unaltered image by ImageSharp.</param>
    /// <param name="level">The level if the texture is 3d or an array of 2d images. Can be ignored for 2d textures</param>
    public void Write(Image<Rgba32> image, int level = 0) {
        if (Width > 0 && Height > 0 && (image.Width != Width || image.Height != Height))
            Logger.Warn("Supplied image doesn't fit the previously supplied dimensions");
            
        Width = image.Width;
        Height = image.Height;


        GL.BindTexture(Target, Handle);
        image.Mutate(x => x.Flip(FlipMode.Vertical));
        byte[] pixels = new byte[4 * image.Width * image.Height];
        image.CopyPixelDataTo(pixels);
        GlLogger.WriteGLError();
        
        if (Target == TextureTarget.Texture2D) {
            if (IsReserved)
                GL.TexSubImage2D(Target, 0, 0, 0, image.Width, image.Height, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            else
                GL.TexImage2D(Target, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        }
        else if (Target == TextureTarget.Texture2DArray) {
            if (!IsReserved) {
                Logger.Warn("Can't write to 3d texture if no reservation is made");
                return;
            }
            else
                GL.TexSubImage3D(Target, 0, 0, 0, level, image.Width, image.Height, 1, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        }
        GlLogger.WriteGLError();

        GenerateMipmapTarget gmt = Target switch {
            TextureTarget.Texture2DArray => GenerateMipmapTarget.Texture2DArray,
            _ => GenerateMipmapTarget.Texture2D
        };
        GL.GenerateMipmap(gmt);
        GlLogger.WriteGLError();
    }

    /// <summary>
    /// Reserves memory on the gpu. Necessary if you can't provide the entire texture in one write call, for example when using
    /// texture 2d arrays.
    /// </summary>
    /// <param name="width">The width of all images that should be stored</param>
    /// <param name="height">The height of all images that should be stored</param>
    /// <param name="depth">The depth of the texture, the number of layers for 2d arrays or 3d textures. Can be ignored for a normal 2d texture</param>
    public void Reserve(int width, int height, int depth = 0) {
        Width = width;
        Height = height;


        GL.BindTexture(Target, Handle);
        if (Target == TextureTarget.Texture2D) {
            GL.TexStorage2D(TextureTarget2d.Texture2D, 1, SizedInternalFormat.Rgba8, width, height);
        }
        else if (Target == TextureTarget.Texture2DArray)
            GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1, SizedInternalFormat.Rgba8, width, height, depth);

        GlLogger.WriteGLError();
        IsReserved = true;
    }

    /// <summary>
    /// Binds the texture to its provided target. Does not handle texture unit, it's a pure bind call
    /// </summary>
    public void Bind() {
        GL.BindTexture(Target, Handle);
    }

    public void Dispose() {
        GL.DeleteTexture(Handle);
        Handle = 0;
    }
}