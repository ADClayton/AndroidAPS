#if ANDROID
using Android.App;
using Android.Content;
using Android.Util;
using AndroidAPSMaui.Services;

namespace AndroidAPSMaui.Platforms.Android.Receivers;

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] { XdripIngestionService.ActionNewBgEstimate, XdripIngestionService.ActionBgEstimateNoData })]
public class XdripBroadcastReceiver : BroadcastReceiver
{
    private const string Tag = "AAPS.MAUI.BgReceiver";

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

        var handler = ServiceResolver.Resolve<XdripIngestionService>();
        if (handler == null)
        {
            Log.Error(Tag, "XdripBroadcastReceiver could not resolve XdripIngestionService. Confirm MauiProgram registers it and the app is initialized before broadcasts arrive.");
            return;
        }

        handler.HandleIntent(intent);
    }
}
#endif
