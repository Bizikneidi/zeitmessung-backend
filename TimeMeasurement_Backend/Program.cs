using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace TimeMeasurement_Backend
{
    /// <summary>
    /// The entry point of the application
    /// </summary>
    public class Program
    {
        public static IWebHost BuildWebHost(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .Build(); //Use appsettings.json

            return WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    //Listen on 5000 with http
                    options.Listen(IPAddress.Any, 5000);
                    try {
                        //Listen on 5001 with https
                        options.Listen(IPAddress.Any, 5001, listenOptions =>
                        {
                            listenOptions.UseHttps("/root/certs/certificate.p12", "Admin1234");
                        });
                    } catch (Exception ex) {
                        Console.WriteLine(ex);
                    }
                })
                .UseStartup<Startup>()
                .UseConfiguration(config)
                .Build();
        }


        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }
    }
}
