using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PACTracker.Infrastructure.Repositories;

namespace PACTracker.Infrastructure.Injection
{
    public static class InfrastructureServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string databasePath)
        {
            services.AddDbContextFactory<AppDbContext>(options =>
            {
                options.UseSqlite($"Data Source={databasePath}");
#if DEBUG
                // Prints the generated SQL in the Output window: great for learning EF Core
                options.LogTo(message => System.Diagnostics.Debug.WriteLine(message),
                    Microsoft.Extensions.Logging.LogLevel.Information);
#endif
            });

            services.AddSingleton<IEnergyRepository, EnergyRepository>();
            return services;
        }

        // Creates the database file and tables if they do not exist yet
        public static void InitializeDatabase(this IServiceProvider provider)
        {
            var factory = provider.GetRequiredService<IDbContextFactory<AppDbContext>>();
            using var db = factory.CreateDbContext();
            db.Database.EnsureCreated();
        }
    }
}
