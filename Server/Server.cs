using Bergmann.Shared;
using Bergmann.Shared.Networking;
using Bergmann.Shared.Networking.Resolvers;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;

namespace Bergmann.Server;

public class Server {
    public static IHubContext<TrueHub> HubContext { get; set; }
    public static IHubClients Clients => HubContext.Clients;
    public static async Task SendToClientAsync(IClientProxy clients, IMessage message) {
        await clients.SendAsync("ServerToClient", MessageBox.Create(message));
    }

    public static void Main(string[] args) {
        Logger.Info("Starting server...");


        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSignalR()
            .AddMessagePackProtocol(options => {
                StaticCompositeResolver.Instance.Register(
                    GeneratedResolver.Instance,
                    CustomResolver.Instance,
                    StandardResolver.Instance
                );

                options.SerializerOptions = 
                    MessagePackSerializerOptions.Standard
                    .WithResolver(StaticCompositeResolver.Instance)
                    .WithSecurity(MessagePackSecurity.UntrustedData);
            })
            .AddHubOptions<TrueHub>(options => {
                options.EnableDetailedErrors = true;
            });
        builder.Services.AddResponseCompression(opts => {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "application/octet-stream" });

        });
        
        builder.WebHost.UseUrls($"http://*:{Names.DefaultPort}");

        WebApplication app = builder.Build();
        app.MapHub<TrueHub>("/" + Names.Hub);
        app.UseRouting();


        HubContext = app.Services.GetService<IHubContext<TrueHub>>()!;


        app.Run();
        Logger.Info("Exiting server");
    }
}