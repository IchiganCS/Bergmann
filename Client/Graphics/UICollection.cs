using Bergmann.Client.Graphics.OpenGL;
using Bergmann.Client.Graphics.Renderers;

namespace Bergmann.Client.Graphics;


/// <summary>
/// This class should do all the UI rendering and event handling in a unified manner. This of course isn't the most optimized way since each held renderer
/// makes its own draw call. But since we don't display thousands of ui elements, it's fine. This class disposes all elements when dispose is called.
/// </summary>
public class UICollection : IDisposable, IRenderer {

    /// <summary>
    /// Stores all ui textures that could be rendered sometimes. This doesn't include letter stacks or block previews,
    /// just normal images like the cross, health and item bar.
    /// </summary>
    private Texture? ImageElements { get; set; }

    /// <summary>
    /// A collection for all rendereres responsible for ui. It is rendered with <see cref="ImageElements"/> bound. 
    /// Text and item preview renderers should not be included. The boolean represents the enabled state of the renderers.
    /// </summary>
    public List<(IUIRenderer, bool)> ImageRenderers { get; set; }

    /// <summary>
    /// A collection of all renderers that provide their own textures. E.g. text renderers or item previews.
    /// The index specifies in which order the renderes are rendered and which ones are on top (last is on top).
    /// The boolean represents the enabled state of the renderers.
    /// </summary>
    public List<(IUIRenderer, bool)> OtherRenderers { get; set; }


    /// <summary>
    /// Constructs a ui collection and stores the image elements.
    /// </summary>
    /// <param name="imgElements">The image elements used to renderer all <see cref="ImageRenderers"/></param>
    public UICollection(Texture? imgElements) {
        ImageElements = imgElements;
        ImageRenderers = new();
        OtherRenderers = new();
    }


    /// <summary>
    /// Make sure the ui program is bound.
    /// </summary>
    public void Render() {
        //rendering order matters. text should be rendered last to be always on top.
        //item previews second.

        ImageElements?.Bind();
        foreach ((IRenderer, bool) r in ImageRenderers)
            if (r.Item2)
                r.Item1.Render();
        
        foreach ((IRenderer, bool) r in OtherRenderers)
            if (r.Item2)
                r.Item1.Render();
    }

    /// <summary>
    /// Disposes all objects. It's not mandatory to call since the class doesn't hold private values which need to be disposed of,
    /// but it may be comfortable
    /// </summary>
    public void Dispose() {
        ImageElements?.Dispose();

        foreach ((IRenderer, bool) r in OtherRenderers)
            r.Item1.Dispose();

        foreach ((IRenderer, bool) r in ImageRenderers)
            r.Item1.Dispose();
    }
}