using Bergmann.Shared;
using Bergmann.Shared.Networking;
using Bergmann.Shared.Networking.Messages;
using Bergmann.Shared.Networking.Resolvers;
using Bergmann.Shared.Networking.Server;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;

namespace Bergmann.Server;

public class Server {
    public static IHubContext<TrueHub> HubContext { get; set; } = null!;
    public static IHubClients Clients => HubContext.Clients;
    
    public static async Task Send(IClientProxy clients, IMessage message) {
        await clients.SendAsync("ServerToClient", new ServerMessageBox(message));
    }

    public static void Main(string[] args) {
        Logger.Info("Starting server...");
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);


        StaticCompositeResolver.Instance.Register(
            GeneratedResolver.Instance,
            OpenTKResolver.Instance,
            StandardResolver.Instance
        );
        
        builder.Services.AddSignalR()
            .AddMessagePackProtocol(options => {

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
        
        builder.WebHost.UseUrls($"http://*:23156");

        WebApplication app = builder.Build();
        app.MapHub<TrueHub>("/Hub");
        app.UseRouting();


        HubContext = app.Services.GetService<IHubContext<TrueHub>>()!;


        app.Run();
        Logger.Info("Exiting server");
    }
}