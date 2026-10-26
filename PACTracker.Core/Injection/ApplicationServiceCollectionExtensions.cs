using Microsoft.Extensions.DependencyInjection;
using PACTracker.Core.Energy.Services;

namespace PACTracker.Core.Injection
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddPACTrackerCoreService(this IServiceCollection services)
        {
            // Singleton only while EnergyService keeps data in memory;
            // switch to AddScoped once it uses the repository
            services.AddSingleton<IEnergyService, EnergyService>();
            return services;
        }
    }
}
