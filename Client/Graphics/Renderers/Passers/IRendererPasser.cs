using OpenTK.Mathematics;

namespace Bergmann.Client.Graphics.Renderers.Passers;

/// <summary>
/// An interface for all renderers who need their own render pass.
/// </summary>
public interface IRendererPasser : IDisposable {
    public void Render(IrregularBox box);
}