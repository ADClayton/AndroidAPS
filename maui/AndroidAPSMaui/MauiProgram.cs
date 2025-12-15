using AndroidAPSMaui.Logging;
using AndroidAPSMaui.Services;
using AndroidAPSMaui.Services.Android;
using AndroidAPSMaui.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AndroidAPSMaui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                // Add custom fonts here if needed
            });

#if ANDROID
        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new Platforms.Android.Logging.LogcatLoggerProvider(MauiLog.Tag));
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

        builder.Services.AddSingleton<ServiceResolver>();
        builder.Services.AddSingleton<BgReadingStore>();
        builder.Services.AddSingleton<PumpEventStore>();
        builder.Services.AddSingleton<BgSmoothingService>();
        builder.Services.AddSingleton<XdripIngestionService>();
        builder.Services.AddSingleton<DashPumpService>();
        builder.Services.AddSingleton<PermissionService>();
        builder.Services.AddSingleton<Graphs.BgGraphDrawable>();
        builder.Services.AddSingleton<GraphViewModel>();
        builder.Services.AddSingleton<PumpConnectionViewModel>();
#if ANDROID
        builder.Services.AddSingleton<IDashPumpTransport, AndroidDashPumpTransport>();
        builder.Services.AddSingleton<IBluetoothDeviceScanner, AndroidBluetoothDeviceScanner>();
#else
        builder.Services.AddSingleton<IBluetoothDeviceScanner, DefaultBluetoothDeviceScanner>();
#endif

        builder.Services.AddTransient<Pages.MainPage>();
        builder.Services.AddTransient<Pages.PumpConnectionPage>();

        var app = builder.Build();
        ServiceResolver.Initialize(app.Services);

#if ANDROID
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("Startup");
        logger.LogInformation("MAUI app configured; logcat tag '{Tag}' active at minimum level {Level}.", MauiLog.Tag, LogLevel.Debug);
#endif
        return app;
    }
}
