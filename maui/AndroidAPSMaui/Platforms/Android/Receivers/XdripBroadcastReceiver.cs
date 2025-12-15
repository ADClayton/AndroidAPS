#if ANDROID
using System;
using Android.App;
using Android.Content;
using Android.Util;
using Microsoft.Maui;
using AndroidAPSMaui.Services;

namespace AndroidAPSMaui.Platforms.Android.Receivers;

[BroadcastReceiver(Enabled = true, Exported = true, DirectBootAware = true)]
[IntentFilter(new[] { XdripIngestionService.ActionNewBgEstimate, XdripIngestionService.ActionBgEstimateNoData })]
public class XdripBroadcastReceiver : BroadcastReceiver
{
    private const string Tag = "AAPS.MAUI.BgReceiver";

    static XdripBroadcastReceiver()
    {
        var receiverAttribute = (BroadcastReceiverAttribute?)Attribute.GetCustomAttribute(
            typeof(XdripBroadcastReceiver), typeof(BroadcastReceiverAttribute));
        var intentFilters = (IntentFilterAttribute[])Attribute.GetCustomAttributes(
            typeof(XdripBroadcastReceiver), typeof(IntentFilterAttribute));

        if (receiverAttribute == null)
        {
            Log.Warn(Tag, "XdripBroadcastReceiver loaded without BroadcastReceiverAttribute. Manifest merge may have failed.");
        }
        else
        {
            Log.Info(Tag,
                $"XdripBroadcastReceiver registration loaded (Enabled={receiverAttribute.Enabled}, Exported={receiverAttribute.Exported}).");
        }

        foreach (var filter in intentFilters)
        {
            Log.Info(Tag, $"IntentFilter actions: {string.Join(",", filter.Actions ?? Array.Empty<string>())}");
        }
    }

    public XdripBroadcastReceiver()
    {
        Log.Debug(Tag, "XdripBroadcastReceiver instance constructed; waiting for broadcasts.");
    }

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context == null)
        {
            Log.Warn(Tag, "XdripBroadcastReceiver triggered with null context. Ensure receiver manifest merge is correct.");
            return;
        }

        if (intent == null)
        {
            Log.Warn(Tag, "XdripBroadcastReceiver received null intent. Sender may not be delivering broadcast correctly.");
            return;
        }

        var action = intent.Action ?? "<null>";
        Log.Info(Tag, $"XdripBroadcastReceiver invoked for action={action} extras={intent.Extras?.KeySet()?.Count ?? 0}");

        var handler = EnsureHandlerInitialized(context);
        if (handler == null)
        {
            Log.Error(Tag, "XdripBroadcastReceiver could not resolve XdripIngestionService. Confirm MauiProgram registers it and the app is initialized before broadcasts arrive.");
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
            Log.Info(Tag, "ServiceResolver initialized from MauiApplication for background broadcast handling.");
            handler = ServiceResolver.Resolve<XdripIngestionService>();
        }
        else
        {
            Log.Warn(Tag, "Unable to obtain MauiApplication services while handling broadcast in background.");
        }

        return handler;
    }
}
#endif
