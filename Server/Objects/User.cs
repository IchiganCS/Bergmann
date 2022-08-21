using Bergmann.Shared.Networking.Client;
using Bergmann.Shared.Networking.Messages;
using Bergmann.Shared.Networking.Server;
using Microsoft.AspNetCore.SignalR;

namespace Bergmann.Server.Objects;

public class User {
    private SortedList<Guid, string> LoggedIn { get; init; } = new();

    public bool IsLoggedIn(Guid? id) {
        if (id is not null)
            return LoggedIn.ContainsKey((Guid)id);
            
        return false;
    }

    public string? GetName(Guid id) {
        if (LoggedIn.TryGetValue(id, out string? val))
            return val;

        return null;
    }

    public async Task HandleMessage(IHubCallerClients clients, LogInAttemptMessage message) {
        Guid id = Guid.NewGuid();
        LoggedIn.Add(id, message.Name);
        await Server.Send(clients.Caller, new SuccessfulLoginMessage(id, message.Name));
    }
}