using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace TimeMeasurement_Backend
{
    public class Program
    {
        public static IWebHost BuildWebHost(string[] args)
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("hosting.json", optional: false)
                .Build();

            return WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
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