using AndroidAPSMaui.Data;

namespace AndroidAPSMaui.Services;

public partial class XdripIngestionService
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
}
