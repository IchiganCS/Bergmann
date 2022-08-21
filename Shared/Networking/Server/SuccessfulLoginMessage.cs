using Bergmann.Shared.Networking.Messages;

namespace Bergmann.Shared.Networking.Server;

[MessagePack.MessagePackObject]
public class SuccessfulLoginMessage : IMessage {
    public Guid UserID { get; init; }
    public string Name { get; init; }


    public SuccessfulLoginMessage(Guid userID, string name) {
        UserID = userID;
        Name = name;
    }
}