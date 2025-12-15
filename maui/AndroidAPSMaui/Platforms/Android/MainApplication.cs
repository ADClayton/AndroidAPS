using Android.App;
using Android.Runtime;
using Android.Util;
using AndroidAPSMaui.Logging;

namespace AndroidAPSMaui;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
    {
    }

    public override void OnCreate()
    {
        base.OnCreate();
        Log.Info(MauiLog.Tag, "MainApplication created; MAUI app is initializing with unified logcat tag.");
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
