using Bergmann.Server.Hubs;
using Bergmann.Shared;
using Bergmann.Shared.Networking;
using MessagePack;
using Microsoft.AspNetCore.ResponseCompression;

namespace Bergmann.Server;

public class Server {
    public static void Main(string[] args) {
        Logger.Info("Starting server...");


        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSignalR()
            .AddMessagePackProtocol(options => {
                options.SerializerOptions = 
                    MessagePackSerializerOptions.Standard
                    .WithResolver(new CustomResolver())
                    .WithSecurity(MessagePackSecurity.UntrustedData);
            });
        builder.Services.AddResponseCompression(opts => {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "application/octet-stream" });

        });
        
        builder.WebHost.UseUrls($"http://*:{Names.DefaultPort}");

        WebApplication app = builder.Build();

        app.UseRouting();
        app.MapHub<ChatHub>("/" + Names.ChatHub);
        app.MapHub<WorldHub>("/" + Names.WorldHub);


        app.Run();
        Logger.Info("Exiting server");
    }
}