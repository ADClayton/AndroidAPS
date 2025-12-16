using Microsoft.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using OmnipodDashController.Services;
using OmnipodDashController.ViewModels;
using OmnipodDashController.Views;

namespace OmnipodDashController;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular"); });

        builder.Services.AddSingleton<IOmnipodDashClient, OmnipodDashClient>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}
