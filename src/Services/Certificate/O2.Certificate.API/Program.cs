using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using O2.Certificate.API.StartupHelpers;

namespace O2.Certificate.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("============== O2 Certificate API =====================");
            var host = CreateHostBuilder(args).Build();
            await host.EnsureDbUpdateToDateUpdateAsync();
            host.Run();
            Console.WriteLine("============== O2 Certificate API - state is started =====================");
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // webBuilder.UseEnvironment("Development");
                    webBuilder.UseStartup<Startup>();
                });
    }
}
