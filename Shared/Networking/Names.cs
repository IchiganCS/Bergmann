namespace Bergmann.Shared.Networking;


/// <summary>
/// This is a helper class to save some constants. Right now, it is does not do very much.
/// </summary>
public static class Constants {

    /// <summary>
    /// The default port used by server and client.
    /// </summary>
    public const int DefaultPort = 23156;
    
    /// <summary>
    /// The name of the hub exchanged between the client and server.
    /// </summary>
    public const string Hub = nameof(Hub);
}