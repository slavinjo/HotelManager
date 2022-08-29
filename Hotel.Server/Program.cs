using System;
using Hotel.Server.Helpers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Sentry;
using Sentry.AspNetCore;

namespace Hotel.Server;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hotel Server initializing...");
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseSentry(o =>
                {
                    o.AddExceptionFilterForType<CustomException>();
                    o.Dsn = StaticConfiguration.SentryDsn;
                });

                webBuilder.UseStartup<Startup>()
                    .UseUrls("http://localhost:5000")
                    .UseKestrel(options => { options.Limits.MaxRequestBodySize = int.MaxValue; });
            });
}
