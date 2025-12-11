namespace AndroidAPSMaui.Data;

public class DashPumpStatus
{
    public required string ConnectionState { get; init; }
    public required string PodIdentifier { get; init; }
    public DateTime RetrievedAt { get; init; } = DateTime.UtcNow;
    public IReadOnlyCollection<PumpEvent> Events { get; init; } = Array.Empty<PumpEvent>();
    public IReadOnlyDictionary<string, string> Telemetry { get; init; } = new Dictionary<string, string>();
}
