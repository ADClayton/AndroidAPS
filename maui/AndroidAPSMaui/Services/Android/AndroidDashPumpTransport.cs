#if ANDROID
using Android.Bluetooth;
using Android.Runtime;
using AndroidAPSMaui.Data;

namespace AndroidAPSMaui.Services.Android;

public class AndroidDashPumpTransport : Java.Lang.Object, IDashPumpTransport
{
    private const string ServiceUuid = "1a7e-4024-e3ed-4464-8b7e-751e03d0dc5f";
    private const string CmdCharacteristicUuid = "1a7e2441-e3ed-4464-8b7e-751e03d0dc5f";
    private const string DataCharacteristicUuid = "1a7e2442-e3ed-4464-8b7e-751e03d0dc5f";

    private BluetoothGatt? _gatt;
    private BluetoothGattCharacteristic? _cmdCharacteristic;
    private BluetoothGattCharacteristic? _dataCharacteristic;
    private TaskCompletionSource<bool>? _serviceDiscoveryTcs;
    private TaskCompletionSource<byte[]>? _readTcs;
    private TaskCompletionSource<bool>? _commandWriteTcs;

    public string? PodAddress { get; set; }
    public string? PodIdentifier { get; set; }

    public async Task<DashPumpStatus> ReadStatusAsync(CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        if (_gatt == null || _dataCharacteristic == null)
        {
            throw new InvalidOperationException("Pump connection is not ready");
        }

        _readTcs = new TaskCompletionSource<byte[]>();
        using var registration = cancellationToken.Register(() => _readTcs.TrySetCanceled());
        if (!_gatt.ReadCharacteristic(_dataCharacteristic))
        {
            throw new InvalidOperationException("Failed to request pump data read");
        }

        var payload = await _readTcs.Task.ConfigureAwait(false);
        return new DashPumpStatus
        {
            ConnectionState = "Connected",
            PodIdentifier = PodIdentifier ?? PodAddress ?? "Unknown",
            RetrievedAt = DateTime.UtcNow,
            Events = new[] { new PumpEvent(DateTime.UtcNow, "Status", Convert.ToHexString(payload)) },
            Telemetry = new Dictionary<string, string> { { "PayloadLength", payload.Length.ToString() } }
        };
    }

    public async Task SendCommandAsync(DashPumpCommand command, CancellationToken cancellationToken = default)
    {
        await EnsureConnectedAsync(cancellationToken).ConfigureAwait(false);
        if (_gatt == null || _cmdCharacteristic == null)
        {
            throw new InvalidOperationException("Pump connection is not ready");
        }

        _commandWriteTcs = new TaskCompletionSource<bool>();
        using var registration = cancellationToken.Register(() => _commandWriteTcs.TrySetCanceled());
        _cmdCharacteristic.SetValue(command.Payload);
        _cmdCharacteristic.WriteType = GattWriteType.Default;
        if (!_gatt.WriteCharacteristic(_cmdCharacteristic))
        {
            throw new InvalidOperationException("Failed to dispatch pump command");
        }

        await _commandWriteTcs.Task.ConfigureAwait(false);
    }

    private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (_gatt != null && _cmdCharacteristic != null && _dataCharacteristic != null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(PodAddress))
        {
            throw new InvalidOperationException("Dash pod address is not configured");
        }

        var adapter = BluetoothAdapter.DefaultAdapter ?? throw new InvalidOperationException("Bluetooth adapter is unavailable");
        var device = adapter.GetRemoteDevice(PodAddress);
        if (device == null)
        {
            throw new InvalidOperationException($"Unable to resolve device {PodAddress}");
        }

        _serviceDiscoveryTcs = new TaskCompletionSource<bool>();
        using var registration = cancellationToken.Register(() => _serviceDiscoveryTcs.TrySetCanceled());
        var callback = new GattCallback(this);
        _gatt = device.ConnectGatt(null, false, callback);
        await _serviceDiscoveryTcs.Task.ConfigureAwait(false);
    }

    private class GattCallback : BluetoothGattCallback
    {
        private readonly AndroidDashPumpTransport _transport;

        public GattCallback(AndroidDashPumpTransport transport)
        {
            _transport = transport;
        }

        public override void OnConnectionStateChange(BluetoothGatt? gatt, GattStatus status, ProfileState newState)
        {
            base.OnConnectionStateChange(gatt, status, newState);
            if (newState == ProfileState.Connected)
            {
                gatt?.DiscoverServices();
            }
        }

        public override void OnServicesDiscovered(BluetoothGatt? gatt, [GeneratedEnum] GattStatus status)
        {
            base.OnServicesDiscovered(gatt, status);
            if (status != GattStatus.Success || gatt == null)
            {
                _transport._serviceDiscoveryTcs?.TrySetException(new InvalidOperationException($"Service discovery failed: {status}"));
                return;
            }

            var service = gatt.GetService(Java.Util.UUID.FromString(ServiceUuid));
            _transport._cmdCharacteristic = service?.GetCharacteristic(Java.Util.UUID.FromString(CmdCharacteristicUuid));
            _transport._dataCharacteristic = service?.GetCharacteristic(Java.Util.UUID.FromString(DataCharacteristicUuid));

            if (_transport._cmdCharacteristic == null || _transport._dataCharacteristic == null)
            {
                _transport._serviceDiscoveryTcs?.TrySetException(new InvalidOperationException("Dash pump characteristics were not found"));
            }
            else
            {
                _transport._serviceDiscoveryTcs?.TrySetResult(true);
            }
        }

        public override void OnCharacteristicRead(BluetoothGatt? gatt, BluetoothGattCharacteristic? characteristic, [GeneratedEnum] GattStatus status)
        {
            base.OnCharacteristicRead(gatt, characteristic, status);
            if (status == GattStatus.Success && characteristic?.GetValue() is { } payload)
            {
                _transport._readTcs?.TrySetResult(payload);
            }
            else
            {
                _transport._readTcs?.TrySetException(new InvalidOperationException($"Read failed: {status}"));
            }
        }

        public override void OnCharacteristicWrite(BluetoothGatt? gatt, BluetoothGattCharacteristic? characteristic, [GeneratedEnum] GattStatus status)
        {
            base.OnCharacteristicWrite(gatt, characteristic, status);
            if (status == GattStatus.Success)
            {
                _transport._commandWriteTcs?.TrySetResult(true);
            }
            else
            {
                _transport._commandWriteTcs?.TrySetException(new InvalidOperationException($"Write failed: {status}"));
            }
        }
    }
}
#endif
