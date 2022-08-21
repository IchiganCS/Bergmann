using Bergmann.Server.Objects;
using Bergmann.Shared.Networking.Messages;

namespace Bergmann.Server.Objects;

/// <summary>
/// Stores all the data of the server. Accessed by hubs.
/// </summary>
public static class Data {
    /// <summary>
    /// The world to be distributed to every client.
    /// </summary>
    public static World World { get; set; } = new();

    /// <summary>
    /// A history of all sent chat messages.
    /// </summary>
    public static IList<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

    /// <summary>
    /// The generator of the world. It is used to generate chunks for <see cref="World"/>.
    /// </summary>
    public static Generator WorldGen { get; set; } = new(Random.Shared.Next());
}