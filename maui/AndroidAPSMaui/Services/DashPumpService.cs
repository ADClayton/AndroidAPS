using AndroidAPSMaui.Data;

namespace AndroidAPSMaui.Services;

public class DashPumpService
{
    private readonly IEnumerable<IDashPumpTransport> _transports;
    private readonly PumpEventStore _eventStore;

    public DashPumpService(IEnumerable<IDashPumpTransport> transports, PumpEventStore eventStore)
    {
        _transports = transports;
        _eventStore = eventStore;
    }

    private IDashPumpTransport ResolveTransport()
    {
        var transport = _transports.FirstOrDefault();
        if (transport == null)
        {
            throw new InvalidOperationException("No Dash pump transport is registered for this platform.");
        }
        return transport;
    }

    public async Task<DashPumpStatus> ReadStatusAsync(CancellationToken cancellationToken = default)
    {
        var transport = ResolveTransport();
        var status = await transport.ReadStatusAsync(cancellationToken).ConfigureAwait(false);
        _eventStore.AddEvents(status.Events);
        return status;
    }

    public async Task SendCommandAsync(DashPumpCommand command, CancellationToken cancellationToken = default)
    {
        var transport = ResolveTransport();
        await transport.SendCommandAsync(command, cancellationToken).ConfigureAwait(false);
        _eventStore.AddEvent(new PumpEvent(DateTime.UtcNow, "Command", command.Description));
    }

    public void Configure(string? address, string? identifier)
    {
        foreach (var transport in _transports)
        {
            transport.PodAddress = address;
            transport.PodIdentifier = identifier;
        }
    }
}
