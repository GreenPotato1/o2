using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using O2.Business.Data;

namespace O2.Business.API.StartupHelpers
{
    internal static class DatabaseExtensions
    {
        internal static async Task EnsureDbUpdateToDateUpdateAsync(this IHost host)
        {
            // using var scope = host.Services.CreateScope();
            // var context = scope.ServiceProvider.GetRequiredService<O2BusinessDataContext>();
            // await context.Database.MigrateAsync();
        }
    }
}