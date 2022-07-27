namespace Bergmann.Shared.Networking;


/// <summary>
/// A collection of strings. Since SignalR only supports weak remote method invocation, it is useful to 
/// at least unify the names.
/// </summary>
public abstract class Names {

    public const string ChatHub = nameof(ChatHub);
    public const string WorldHub = nameof(WorldHub);


    public const string RequestChunk = nameof(RequestChunk);
    public const string DestroyBlock = nameof(DestroyBlock);
    public const string ReceiveChunk = nameof(ReceiveChunk);

    public const string SendMessage = nameof(SendMessage);
    public const string DropWorld = nameof(DropWorld);
}