using Bergmann.Shared.Networking.Messages;

namespace Bergmann.Shared.Networking.Client;

[MessagePack.MessagePackObject]
/// <summary>
/// Sent from the client to attempt to log in.
/// </summary>
public class LogInAttemptMessage : IMessage {
    public string Password { get; init; }
    public string Name { get; init; }

    public LogInAttemptMessage(string password, string name) {
        Password = password;
        Name = name;
    }
}