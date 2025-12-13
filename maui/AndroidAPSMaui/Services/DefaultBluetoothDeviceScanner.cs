#if !ANDROID
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AndroidAPSMaui.Data;

namespace AndroidAPSMaui.Services;

public class DefaultBluetoothDeviceScanner : IBluetoothDeviceScanner
{
    public Task<IReadOnlyList<BluetoothDeviceInfo>> ScanAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<BluetoothDeviceInfo>>(Array.Empty<BluetoothDeviceInfo>());
    }
}
#endif
