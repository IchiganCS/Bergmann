using Bergmann.Shared.Networking.Messages;
using Bergmann.Shared.Objects;

namespace Bergmann.Shared.Networking.Server;

[MessagePack.MessagePackObject]
/// <summary>
/// This message packs the sending of an entire chunk. Since consumes quite a lot of bandwith and 
/// computational power, this is only to be sent when absolutely necessary, for example when a client
/// requests information about a chunk and the server needs to tell the client.
/// </summary>
public class RawChunkMessage : IMessage {

    /// <summary>
    /// The chunk which is sent. It might not necessarily be up to date when received 
    /// (as are all messages).
    /// </summary>
    public Chunk Chunk { get; set; }

    public RawChunkMessage(Chunk chunk) {
        Chunk = chunk;
    }
}