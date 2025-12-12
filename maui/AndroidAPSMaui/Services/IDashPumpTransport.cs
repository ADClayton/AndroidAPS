using AndroidAPSMaui.Data;

namespace AndroidAPSMaui.Services;

public interface IDashPumpTransport
{
    Task<DashPumpStatus> ReadStatusAsync(CancellationToken cancellationToken = default);
    Task SendCommandAsync(DashPumpCommand command, CancellationToken cancellationToken = default);
    string? PodAddress { get; set; }
    string? PodIdentifier { get; set; }
}
