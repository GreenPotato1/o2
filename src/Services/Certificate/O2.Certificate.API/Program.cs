using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using O2.Certificate.API.Helper;
using O2.Certificate.API.StartupHelpers;

namespace O2.Certificate.API
{
    public class Program
    {
        public static readonly string Namespace = typeof(Program).Namespace;
        public static readonly string AppName = Namespace.Substring(Namespace.LastIndexOf('.', Namespace.LastIndexOf('.') - 1) + 1);
        
        public static async Task Main(string[] args)
        {
            Console.WriteLine("============== O2 Certificate API =====================");
            var host = CreateHostBuilder(args).Build();
            await host.EnsureDbUpdateToDateUpdateAsync();
            Console.WriteLine("============== O2 Certificate API - state is started =====================");
            host.Run();
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
