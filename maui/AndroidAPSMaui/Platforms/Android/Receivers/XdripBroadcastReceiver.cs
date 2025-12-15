#if ANDROID
using System;
using Android.App;
using Android.Content;
using Android.Util;
using Microsoft.Maui;
using AndroidAPSMaui.Services;
using AndroidAPSMaui.Logging;

namespace AndroidAPSMaui.Platforms.Android.Receivers;

[BroadcastReceiver(Enabled = true, Exported = true, DirectBootAware = true, Permission = XdripIngestionService.PermissionReceiveBgEstimate)]
[IntentFilter(new[] { XdripIngestionService.ActionNewBgEstimate, XdripIngestionService.ActionBgEstimateNoData })]
public class XdripBroadcastReceiver : BroadcastReceiver
{

    static XdripBroadcastReceiver()
    {
        var receiverAttribute = (BroadcastReceiverAttribute?)Attribute.GetCustomAttribute(
            typeof(XdripBroadcastReceiver), typeof(BroadcastReceiverAttribute));
        var intentFilters = (IntentFilterAttribute[])Attribute.GetCustomAttributes(
            typeof(XdripBroadcastReceiver), typeof(IntentFilterAttribute));

        if (receiverAttribute == null)
        {
            Log.Warn(MauiLog.Tag, "XdripBroadcastReceiver loaded without BroadcastReceiverAttribute. Manifest merge may have failed.");
        }
        else
        {
            Log.Info(MauiLog.Tag,
                $"XdripBroadcastReceiver registration loaded (Enabled={receiverAttribute.Enabled}, Exported={receiverAttribute.Exported}).");
        }

        foreach (var filter in intentFilters)
        {
            Log.Info(MauiLog.Tag, $"IntentFilter actions: {string.Join(",", filter.Actions ?? Array.Empty<string>())}");
        }
    }

    public XdripBroadcastReceiver()
    {
        Log.Debug(MauiLog.Tag, "XdripBroadcastReceiver instance constructed; waiting for broadcasts.");
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null)
        {
            Log.Warn(MauiLog.Tag, "XdripBroadcastReceiver triggered with null context. Ensure receiver manifest merge is correct.");
            return;
        }

        if (intent == null)
        {
            Log.Warn(MauiLog.Tag, "XdripBroadcastReceiver received null intent. Sender may not be delivering broadcast correctly.");
            return;
        }

        var action = intent.Action ?? "<null>";
        Log.Info(MauiLog.Tag, $"XdripBroadcastReceiver invoked for action={action} extras={intent.Extras?.KeySet()?.Count ?? 0}");

        var handler = EnsureHandlerInitialized(context);
        if (handler == null)
        {
            Log.Error(MauiLog.Tag, "XdripBroadcastReceiver could not resolve XdripIngestionService. Confirm MauiProgram registers it and the app is initialized before broadcasts arrive.");
            return;
        }

        handler.HandleIntent(intent);
    }

    private static XdripIngestionService? EnsureHandlerInitialized(Context context)
    {
        var handler = ServiceResolver.Resolve<XdripIngestionService>();
        if (handler != null)
        {
            return handler;
        }

        if (context.ApplicationContext is MauiApplication mauiApp && mauiApp.Services != null)
        {
            ServiceResolver.Initialize(mauiApp.Services);
            Log.Info(MauiLog.Tag, "ServiceResolver initialized from MauiApplication for background broadcast handling.");
            handler = ServiceResolver.Resolve<XdripIngestionService>();
        }
        else
        {
            Log.Warn(MauiLog.Tag, "Unable to obtain MauiApplication services while handling broadcast in background.");
        }

        return handler;
    }
}
#endif
