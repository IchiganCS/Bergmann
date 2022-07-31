using Bergmann.Shared.World;

namespace Bergmann.Server;

/// <summary>
/// Stores all the data of the server. Accessed by hubs.
/// </summary>
public static class Data {
    /// <summary>
    /// The world to be distributed to every client.
    /// </summary>
    public static World World { get; set; } = new();

    /// <summary>
    /// The generator of the world. It is used to generate chunks for <see cref="World"/>.
    /// </summary>
    /// <returns></returns>
    public static Generator WorldGen { get; set; } = new(Random.Shared.Next());
}