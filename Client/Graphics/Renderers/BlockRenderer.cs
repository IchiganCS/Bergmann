using Bergmann.Client.Graphics.OpenGL;
using OpenTK.Graphics.OpenGL;

namespace Bergmann.Client.Graphics.Renderers;

/// <summary>
/// This class does not render a block. Instead, it only holds necessary information, such as the textures.
/// It could also be added to Block.cs but let's keep shared clean of any graphical issues.
/// </summary>
public static class BlockRenderer {
    public static Texture TextureStack;

    public static void StackTextures() {
        TextureStack = new(TextureTarget.Texture2DArray);
    }
}