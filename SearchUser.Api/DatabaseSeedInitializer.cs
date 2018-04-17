using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SearchUser.Api.Initializer;
using SearchUser.Api.Persistence;
using System;
using System.Threading.Tasks;

namespace SearchUser.Api
{
    public static class DatabaseSeedInitializer
    {
        public static IWebHost Seed(this IWebHost host)
        {
            using (var serviceScope = host.Services.CreateScope())
            {
                var serviceProvider = serviceScope.ServiceProvider;
                var hostingEnvironment = serviceProvider.GetService<IHostingEnvironment>();
                if (!hostingEnvironment.IsDevelopment()) return host;

                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                try
                {
                    Task.Run(async () =>
                    {
                        logger.LogDebug("Seeding the DB with test data.");

                        var dataseed = new DatabaseInitializer();
                        await dataseed.InitializeDataAsync(serviceProvider);
                    }).Wait();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }
            }
            return host;
        }
    }
}
