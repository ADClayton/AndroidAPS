using AndroidAPSMaui.Data;

namespace AndroidAPSMaui.Services;

public class XdripIngestionService
{
    public const string ActionNewBgEstimate = "com.eveningoutpost.dexdrip.BgEstimate";
    public const string ActionBgEstimateNoData = "com.eveningoutpost.dexdrip.BgEstimateNoData";
    public const string ExtraTimestamp = "com.eveningoutpost.dexdrip.Extras.Time";
    public const string ExtraBgEstimate = "com.eveningoutpost.dexdrip.Extras.BgEstimate";
    public const string ExtraBgSlopeName = "com.eveningoutpost.dexdrip.Extras.BgSlopeName";
    public const string ExtraRaw = "com.eveningoutpost.dexdrip.Extras.Raw";
    public const string ExtraSource = "com.eveningoutpost.dexdrip.Extras.SourceInfo";

    private readonly BgReadingStore _bgReadingStore;
    private readonly BgSmoothingService _smoothingService;

    public XdripIngestionService(BgReadingStore bgReadingStore, BgSmoothingService smoothingService)
    {
        _bgReadingStore = bgReadingStore;
        _smoothingService = smoothingService;
    }

#if ANDROID
    public void HandleIntent(Android.Content.Intent? intent)
    {
        if (intent == null)
        {
            return;
        }

        if (intent.Action != ActionNewBgEstimate && intent.Action != ActionBgEstimateNoData)
        {
            return;
        }

        var timestamp = intent.GetLongExtra(ExtraTimestamp, 0);
        if (timestamp == 0)
        {
            return;
        }

        var value = intent.GetDoubleExtra(ExtraBgEstimate, 0);
        var raw = intent.GetDoubleExtra(ExtraRaw, double.NaN);
        var slope = intent.GetStringExtra(ExtraBgSlopeName);
        var source = intent.GetStringExtra(ExtraSource);
        var reading = new BgReading(DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime, value, double.IsNaN(raw) ? null : raw, source, slope);
        var smoothed = _smoothingService.Smooth(_bgReadingStore.Readings.Append(reading));
        _bgReadingStore.AddReadings(smoothed.TakeLast(1));
    }
#endif
}
