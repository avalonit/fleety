using AITrackerAgent.Services;
using AITrackerAgent.Interfaces;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Core.Hosting;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using AITrackerAgent.Classes;

namespace AITrackerAgent
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddTransient<AITrackerAgentViewModel>();
            builder.Services.AddTransient<IAgentService, AgentService>();
            builder.Services.AddTransient<IAddressService, AddressServices>();
            builder.Services.AddTransient<ICommunicationService, CommunicationService>();

            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("AITrackerAgent.appsettings.json");
#pragma warning disable CS8604 // Possible null reference argument.
            var configuration = new ConfigurationBuilder()
                        .AddJsonStream(stream)
                        .Build();
#pragma warning restore CS8604 // Possible null reference argument.
            builder.Configuration.AddConfiguration(configuration);

#if DEBUG
            builder.Logging.AddDebug();
#endif
            var settings = configuration.Get<AppSettings>();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(settings.SyncfusionLicense);
            return builder.Build();
        }
    }
}
