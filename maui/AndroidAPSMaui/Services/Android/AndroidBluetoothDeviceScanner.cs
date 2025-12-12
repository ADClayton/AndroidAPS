#if ANDROID
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.OS;
using Android.Runtime;
using AndroidAPSMaui.Data;

namespace AndroidAPSMaui.Services.Android;

public class AndroidBluetoothDeviceScanner : IBluetoothDeviceScanner
{
    public async Task<IReadOnlyList<BluetoothDeviceInfo>> ScanAsync(CancellationToken cancellationToken = default)
    {
        var adapter = BluetoothAdapter.DefaultAdapter ?? throw new InvalidOperationException("Bluetooth adapter is unavailable");
        if (!adapter.IsEnabled)
        {
            throw new InvalidOperationException("Enable Bluetooth to scan for pods.");
        }

        var results = new Dictionary<string, BluetoothDeviceInfo>();

        if (adapter.BluetoothLeScanner is { } scanner)
        {
            var callback = new ScanCallbackImpl(results);
            scanner.StartScan(callback);
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                scanner.StopScan(callback);
            }
        }

        if (adapter.BondedDevices is { } bondedDevices)
        {
            foreach (var device in bondedDevices)
            {
                if (!results.ContainsKey(device.Address))
                {
                    results[device.Address] = new BluetoothDeviceInfo(device.Name ?? "Unknown", device.Address);
                }
            }
        }

        return results.Values.ToList();
    }

    private class ScanCallbackImpl : ScanCallback
    {
        private readonly Dictionary<string, BluetoothDeviceInfo> _results;

        public ScanCallbackImpl(Dictionary<string, BluetoothDeviceInfo> results)
        {
            _results = results;
        }

        public override void OnScanResult([GeneratedEnum] ScanCallbackType callbackType, ScanResult? result)
        {
            base.OnScanResult(callbackType, result);
            if (result?.Device?.Address == null)
            {
                return;
            }

            var name = string.IsNullOrWhiteSpace(result.Device.Name) ? "Unknown" : result.Device.Name;
            _results[result.Device.Address] = new BluetoothDeviceInfo(name, result.Device.Address);
        }
    }
}
#endif
