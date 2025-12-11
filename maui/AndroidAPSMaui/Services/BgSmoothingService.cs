using AndroidAPSMaui.Data;

namespace AndroidAPSMaui.Services;

public class BgSmoothingService
{
    private readonly double _alpha;

    public BgSmoothingService(double alpha = 0.35)
    {
        _alpha = Math.Clamp(alpha, 0.01, 1.0);
    }

    public IReadOnlyList<BgReading> Smooth(IEnumerable<BgReading> readings)
    {
        var ordered = readings.OrderBy(r => r.Timestamp).ToList();
        if (ordered.Count == 0)
        {
            return Array.Empty<BgReading>();
        }

        var smoothed = new List<BgReading> { ordered[0] };
        var last = ordered[0].Value;
        for (int i = 1; i < ordered.Count; i++)
        {
            last = _alpha * ordered[i].Value + (1 - _alpha) * last;
            smoothed.Add(new BgReading(ordered[i].Timestamp, Math.Round(last, 1), ordered[i].Raw, ordered[i].SourceSensor, ordered[i].Trend));
        }
        return smoothed;
    }
}
