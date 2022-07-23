using Bergmann.Shared.World;

namespace Bergmann.Server;

/// <summary>
/// Stores all the data of the server. Accessed by hubs.
/// </summary>
public static class Data {
    public static World World { get; set; } = new();
}