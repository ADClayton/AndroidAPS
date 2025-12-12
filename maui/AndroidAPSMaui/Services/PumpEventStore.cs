using AndroidAPSMaui.Data;

namespace AndroidAPSMaui.Services;

public class PumpEventStore
{
    private readonly List<PumpEvent> _events = new();
    private readonly object _gate = new();

    public event EventHandler? EventsChanged;

    public IReadOnlyList<PumpEvent> Events
    {
        get { lock (_gate) { return _events.ToList(); } }
    }

    public void AddEvents(IEnumerable<PumpEvent> events)
    {
        lock (_gate)
        {
            _events.AddRange(events);
            _events.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));
        }
        EventsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void AddEvent(PumpEvent pumpEvent)
    {
        AddEvents(new[] { pumpEvent });
    }
}
