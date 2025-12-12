namespace AndroidAPSMaui.Data;

public record BgReading(DateTime Timestamp, double Value, double? Raw = null, string? SourceSensor = null, string? Trend = null);
