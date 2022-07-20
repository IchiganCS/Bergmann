using Bergmann.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;

namespace Bergmann.Server;

public class Server {
    public static void Main(string[] args) {
        Logger.Info("Starting server...");


        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSignalR();
        builder.Services.AddResponseCompression(opts => {
            opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                new[] { "application/octet-stream" });

        });

        WebApplication app = builder.Build();

        app.UseRouting();
        app.MapHub<ChatHub>("/ChatHub");


        app.Run();
        Logger.Info("Exiting server");
    }
}