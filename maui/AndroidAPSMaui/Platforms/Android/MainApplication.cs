using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Util;
using AndroidAPSMaui.Logging;
using AndroidAPSMaui.Services;

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
        LogReceiverRegistration();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    private void LogReceiverRegistration()
    {
        try
        {
            var pm = PackageManager;
            if (pm == null)
            {
                Log.Warn(MauiLog.Tag, "PackageManager unavailable; cannot verify broadcast receiver registration.");
                return;
            }

            var intent = new Intent(XdripIngestionService.ActionNewBgEstimate);
            intent.SetPackage(PackageName);
            var receivers = pm.QueryBroadcastReceivers(intent, PackageInfoFlags.MatchAll);
            var count = receivers?.Count ?? 0;
            Log.Info(MauiLog.Tag,
                $"QueryBroadcastReceivers for action {XdripIngestionService.ActionNewBgEstimate} returned {count} entries in package {PackageName}.");

            if (receivers != null)
            {
                foreach (var resolveInfo in receivers)
                {
                    var activityInfo = resolveInfo.ActivityInfo;
                    Log.Info(MauiLog.Tag,
                        $"Receiver entry: {activityInfo?.Name ?? "<unknown>"}, Enabled={activityInfo?.Enabled}, Exported={activityInfo?.Exported}, Permission={activityInfo?.Permission ?? "<none>"}.");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(MauiLog.Tag, $"Failed to verify XdripBroadcastReceiver registration: {ex}");
        }
    }
}
