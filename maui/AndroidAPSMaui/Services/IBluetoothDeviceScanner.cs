using AndroidAPSMaui.Data;

namespace AndroidAPSMaui.Services;

public interface IBluetoothDeviceScanner
{
    Task<IReadOnlyList<BluetoothDeviceInfo>> ScanAsync(CancellationToken cancellationToken = default);
}
