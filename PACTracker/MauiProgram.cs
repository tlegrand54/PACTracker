using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.Extensions.Logging;
using PACTracker.Core.Injection;
using PACTracker.Infrastructure.Injection;
using PACTracker.ViewModels;
using PACTracker.Views;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace PACTracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseLiveCharts()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // Add ViewModels and Views
            builder.Services.AddTransient<MainPageViewModel>();
            builder.Services.AddTransient<MainPage>();

            builder.Services.AddTransient<EnergyViewModel>();
            builder.Services.AddTransient<EnergyPage>();

            builder.Services.AddTransient<AddEnergyViewModel>();
            builder.Services.AddTransient<AddEnergyPage>();

            builder.Services.AddTransient<ComparatifGraphViewModel>();
            builder.Services.AddTransient<ComparatifGraphPage>();

            builder.Services.AddTransient<AnnualPageViewModel>();
            builder.Services.AddTransient<AnnualPage>();

            // Add Service from PACTracker.Core.Injection
            builder.Services.AddPACTrackerCoreService();

            // Add Service from PACTracker.Infrastructure.Injection
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "pactracker.db3");
            builder.Services.AddInfrastructure(dbPath);

            var app = builder.Build();

            app.Services.InitializeDatabase();

            return app;
        }
    }
}
