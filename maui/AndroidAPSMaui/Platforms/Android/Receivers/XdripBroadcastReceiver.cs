#if ANDROID
using Android.Content;
using AndroidAPSMaui.Services;

namespace AndroidAPSMaui.Platforms.Android.Receivers;

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] { XdripIngestionService.ActionNewBgEstimate, XdripIngestionService.ActionBgEstimateNoData })]
public class XdripBroadcastReceiver : BroadcastReceiver
{
    public override void OnReceive(Context? context, Intent? intent)
    {
        var handler = ServiceResolver.Resolve<XdripIngestionService>();
        handler?.HandleIntent(intent);
    }
}
#endif
