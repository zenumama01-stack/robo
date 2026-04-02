using Microsoft.AspNetCore.Builder;
using static Microsoft.AspNetCore.Hosting.WebHostBuilderKestrelExtensions;
namespace UnixSocket
            WebApplicationBuilder builder = WebApplication.CreateBuilder();
            builder.WebHost.ConfigureKestrel(options =>
                options.ListenUnixSocket(args[0]);
            var app = builder.Build();
            app.MapGet("/", () => "Hello World Unix Socket.");
            app.Run();
