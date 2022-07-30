namespace Bergmann.Shared.Noise;


/// <summary>
/// A unified interface to sample any noise function.
/// </summary>
/// <typeparam name="T">The type to identify a position by.</typeparam>
public interface INoise<T> {

    /// <summary>
    /// Gets a value from the noise generator. Any value can be submitted, the noise generator needs to handle
    /// all cases.
    /// </summary>
    /// <param name="position">The position to sample the noise by.</param>
    /// <returns>A value in [-1, 1].</returns>
    public float Sample(T position);
}