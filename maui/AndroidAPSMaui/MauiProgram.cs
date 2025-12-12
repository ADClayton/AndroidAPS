using AndroidAPSMaui.Services;
using AndroidAPSMaui.Services.Android;
using AndroidAPSMaui.ViewModels;

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
        return app;
    }
}
