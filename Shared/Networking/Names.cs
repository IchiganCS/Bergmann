namespace Bergmann.Shared.Networking;


/// <summary>
/// A collection of strings. Since SignalR only supports weak remote method invocation, it is useful to 
/// at least unify the names. It also stores any other information related to networking which can be synchronized
/// by a simple variable.
/// </summary>
public static class Names {

    /// <summary>
    /// The default port used by server and client.
    /// </summary>
    public const int DefaultPort = 23156;
    
    public const string Hub = nameof(Hub);
}