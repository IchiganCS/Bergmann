using Bergmann.Shared;
using Bergmann.Shared.Networking;
using MessagePack;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Bergmann.Client;


/// <summary>
/// A unified place of the client to load all hubs. Each hub is resolved
/// using the strings given in <see cref="Names"/>.
/// </summary>
#pragma warning disable 8618
public static class Hubs {
    public static HubConnection World { get; private set; }
    public static HubConnection Chat { get; private set; }

    public static string Link { get; private set; }

    /// <summary>
    /// Builds all hubs in the collection on the specified link
    /// </summary>
    /// <param name="link">Without any trailing slashes, the full protocol, domain and port, e.g. http://localhost:5000</param>
    public static void InitializeWithLink(string link) {
        Link = link;
        Logger.Info("Conencting to " + link);

        Func<string, HubConnection> buildHub = sr => {
            HubConnection hc = new HubConnectionBuilder()
                .WithUrl(Link + "/" + sr)
                .WithAutomaticReconnect()
                .AddMessagePackProtocol(options => {
                    options.SerializerOptions = 
                        MessagePackSerializerOptions.Standard
                        .WithResolver(new CustomResolver())
                        .WithSecurity(MessagePackSecurity.UntrustedData);
                })
                .Build();
            hc.StartAsync();
            return hc;
        };

        World = buildHub(Names.WorldHub);
        Chat = buildHub(Names.ChatHub);
    }
}