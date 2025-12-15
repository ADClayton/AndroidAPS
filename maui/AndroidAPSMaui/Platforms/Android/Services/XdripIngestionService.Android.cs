#if ANDROID
using Android.Content;
using Android.Util;
using AndroidAPSMaui.Data;
using AndroidAPSMaui.Logging;

namespace AndroidAPSMaui.Services;

public partial class XdripIngestionService
{
    public void HandleIntent(Intent? intent)
    {
        if (intent == null)
        {
            Log.Warn(MauiLog.Tag, "XdripIngestionService.HandleIntent invoked with null intent; broadcast may be malformed.");
            return;
        }

        if (intent.Action != ActionNewBgEstimate && intent.Action != ActionBgEstimateNoData)
        {
            Log.Warn(MauiLog.Tag, $"Ignoring intent with unexpected action {intent.Action ?? "<null>"}. Ensure sender matches {ActionNewBgEstimate} or {ActionBgEstimateNoData}.");
            return;
        }

        var timestamp = intent.GetLongExtra(ExtraTimestamp, 0);
        if (timestamp == 0)
        {
            Log.Warn(MauiLog.Tag, "Missing timestamp extra in BG broadcast; cannot persist reading.");
            return;
        }

        var value = intent.GetDoubleExtra(ExtraBgEstimate, 0);
        var raw = intent.GetDoubleExtra(ExtraRaw, double.NaN);
        var slope = intent.GetStringExtra(ExtraBgSlopeName);
        var source = intent.GetStringExtra(ExtraSource);
        var reading = new BgReading(DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime, value, double.IsNaN(raw) ? null : raw, source, slope);
        var smoothed = _smoothingService.Smooth(_bgReadingStore.Readings.Append(reading));
        _bgReadingStore.AddReadings(smoothed.TakeLast(1));
        Log.Info(MauiLog.Tag, $"Stored BG reading {value} mg/dL at {timestamp} from {source ?? "unknown"}, raw={(double.IsNaN(raw) ? "n/a" : raw)} slope={slope ?? "n/a"}.");
    }
}
#endif
