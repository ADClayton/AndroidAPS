using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Util;
using AndroidAPSMaui.Platforms.Android.Receivers;
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

            var permissionState = pm.CheckPermission(XdripIngestionService.PermissionReceiveBgEstimate, PackageName);
            Log.Info(MauiLog.Tag,
                $"DexDrip permission state for package {PackageName}: {permissionState} (0=granted). Ensure this is granted so permission-gated broadcasts can reach the receiver.");

            var receiverComponent = new ComponentName(PackageName, typeof(XdripBroadcastReceiver).FullName!);
            var enabledSetting = pm.GetComponentEnabledSetting(receiverComponent);
            Log.Info(MauiLog.Tag,
                $"ComponentEnabledSetting for {receiverComponent.ClassName}: {enabledSetting} (1=enabled, 2=disabled, default uses manifest).");

            var implicitResolve = pm.ResolveBroadcast(new Intent(XdripIngestionService.ActionNewBgEstimate), PackageInfoFlags.MatchAll);
            if (implicitResolve?.ActivityInfo != null)
            {
                Log.Info(MauiLog.Tag,
                    $"ResolveBroadcast (implicit) returned {implicitResolve.ActivityInfo.Name} (Exported={implicitResolve.ActivityInfo.Exported}, Permission={implicitResolve.ActivityInfo.Permission ?? "<none>"}).");
            }
            else
            {
                Log.Warn(MauiLog.Tag,
                    "ResolveBroadcast (implicit) returned no receiver. If xDrip sends implicit broadcasts, the filter may be missing or app disabled.");
            }

            var explicitIntent = new Intent(XdripIngestionService.ActionNewBgEstimate);
            explicitIntent.SetComponent(receiverComponent);
            var explicitResolve = pm.ResolveBroadcast(explicitIntent, PackageInfoFlags.MatchAll);
            if (explicitResolve?.ActivityInfo != null)
            {
                Log.Info(MauiLog.Tag,
                    $"ResolveBroadcast (explicit) returned {explicitResolve.ActivityInfo.Name} (Exported={explicitResolve.ActivityInfo.Exported}, Permission={explicitResolve.ActivityInfo.Permission ?? "<none>"}).");
            }
            else
            {
                Log.Warn(MauiLog.Tag,
                    "ResolveBroadcast (explicit) returned no receiver. Ensure the component name matches the installed package (check logcat package name vs manifest).");
            }
        }
        catch (Exception ex)
        {
            Log.Error(MauiLog.Tag, $"Failed to verify XdripBroadcastReceiver registration: {ex}");
        }
    }
}
