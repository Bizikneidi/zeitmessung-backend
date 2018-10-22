using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Net;

namespace TimeMeasurement_Backend
{
    public class Program
    {
        public static IWebHost BuildWebHost(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Parse("172.18.2.16"), 5000);
                    options.Listen(IPAddress.Parse("172.18.2.16"), 5001, listenOptions =>
                    {
                        //listenOptions.UseHttps("/root/certs/certificate.p12", "Admin1234");
                    });
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
