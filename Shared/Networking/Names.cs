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
    
    public const string ChatHub = nameof(ChatHub);
    public const string WorldHub = nameof(WorldHub);

    public static class Server {

        public const string RequestChunk = nameof(RequestChunk);
        public const string RequestChunkColumn = nameof(RequestChunkColumn);
        public const string DestroyBlock = nameof(DestroyBlock);
        public const string SendMessage = nameof(SendMessage);
    }


    public static class Client {
        public const string ReceiveChunk = nameof(ReceiveChunk);

        /// <summary>
        /// Sends a message that a chunk has been updated.
        /// 
        /// Args: long key, IList Vector3i positions, IList Block blocks
        /// </summary>
        public const string ReceiveChunkUpdate = nameof(ReceiveChunkUpdate);
        public const string ReceiveMessage = nameof(ReceiveMessage);
    }
}