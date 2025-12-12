using AndroidAPSMaui.Data;

namespace AndroidAPSMaui.Services;

public class BgReadingStore
{
    private readonly List<BgReading> _readings = new();
    private readonly object _gate = new();

    public event EventHandler? ReadingsChanged;

    public IReadOnlyList<BgReading> Readings
    {
        get { lock (_gate) { return _readings.ToList(); } }
    }

    public void AddReading(BgReading reading)
    {
        lock (_gate)
        {
            _readings.Add(reading);
            _readings.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
        }
        ReadingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddReadings(IEnumerable<BgReading> readings)
    {
        lock (_gate)
        {
            _readings.AddRange(readings);
            _readings.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
        }
        ReadingsChanged?.Invoke(this, EventArgs.Empty);
    }
}
