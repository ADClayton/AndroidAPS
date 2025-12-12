using AndroidAPSMaui.Data;
using System.Text.Json;
using System.Linq;

namespace AndroidAPSMaui.Services;

public class BgReadingStore
{
    private const string StorageKey = "bg_readings_cache";
    private readonly List<BgReading> _readings = new();
    private readonly object _gate = new();

    public BgReadingStore()
    {
        TryLoadPersistedReadings();
    }

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
        PersistReadings();
        ReadingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddReadings(IEnumerable<BgReading> readings)
    {
        lock (_gate)
        {
            _readings.AddRange(readings);
            _readings.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
        }
        PersistReadings();
        ReadingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private void PersistReadings()
    {
        try
        {
            var snapshot = Readings.TakeLast(60).ToList();
            var serialized = JsonSerializer.Serialize(snapshot);
            Preferences.Default.Set(StorageKey, serialized);
        }
        catch
        {
            // Persistence is best-effort; ignore serialization errors.
        }
    }

    private void TryLoadPersistedReadings()
    {
        try
        {
            var serialized = Preferences.Default.Get(StorageKey, string.Empty);
            if (string.IsNullOrWhiteSpace(serialized))
            {
                return;
            }

            var restored = JsonSerializer.Deserialize<List<BgReading>>(serialized);
            if (restored == null || restored.Count == 0)
            {
                return;
            }

            lock (_gate)
            {
                _readings.Clear();
                _readings.AddRange(restored.OrderBy(r => r.Timestamp));
            }
        }
        catch
        {
            // Ignore corrupt caches; a fresh reading will repopulate the store.
        }
    }
}
