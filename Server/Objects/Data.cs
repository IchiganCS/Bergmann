using Bergmann.Shared.Networking.Messages;

namespace Bergmann.Server.Objects;

/// <summary>
/// Stores all the data of the server. Accessed by hubs.
/// </summary>
public static class Data {

    /// <summary>
    /// The generator of the world. It is used to generate chunks for <see cref="World"/>.
    /// </summary>
    public static Generator WorldGen { get; set; } = new(Random.Shared.Next());
}