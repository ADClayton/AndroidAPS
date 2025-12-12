using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AndroidAPSMaui.Data;
using AndroidAPSMaui.Services;

namespace AndroidAPSMaui.ViewModels;

public class PumpConnectionViewModel : INotifyPropertyChanged
{
    private readonly DashPumpService _pumpService;
    private readonly PermissionService _permissionService;
    private readonly IBluetoothDeviceScanner _bluetoothDeviceScanner;
    private string? _podAddress;
    private string? _podIdentifier;
    private bool _isScanning;
    private string _scanStatus = "Provide the pod Bluetooth address or pick it from nearby devices.";

    public ObservableCollection<BluetoothDeviceInfo> NearbyDevices { get; } = new();

    public string? PodAddress
    {
        get => _podAddress;
        set => SetProperty(ref _podAddress, value);
    }

    public string? PodIdentifier
    {
        get => _podIdentifier;
        set => SetProperty(ref _podIdentifier, value);
    }

    public bool IsScanning
    {
        get => _isScanning;
        set => SetProperty(ref _isScanning, value);
    }

    public string ScanStatus
    {
        get => _scanStatus;
        set => SetProperty(ref _scanStatus, value);
    }

    public Command SaveCommand { get; }
    public Command ScanDevicesCommand { get; }
    public Command<BluetoothDeviceInfo> SelectDeviceCommand { get; }

    public PumpConnectionViewModel(DashPumpService pumpService, PermissionService permissionService, IBluetoothDeviceScanner bluetoothDeviceScanner)
    {
        _pumpService = pumpService;
        _permissionService = permissionService;
        _bluetoothDeviceScanner = bluetoothDeviceScanner;
        _podAddress = Preferences.Default.Get("dash_pod_address", string.Empty);
        _podIdentifier = Preferences.Default.Get("dash_pod_identifier", string.Empty);
        _pumpService.Configure(_podAddress, _podIdentifier);
        SaveCommand = new Command(Save);
        ScanDevicesCommand = new Command(async () => await ScanAsync());
        SelectDeviceCommand = new Command<BluetoothDeviceInfo>(SelectDevice);
    }

    private void Save()
    {
        Preferences.Default.Set("dash_pod_address", PodAddress ?? string.Empty);
        Preferences.Default.Set("dash_pod_identifier", PodIdentifier ?? string.Empty);
        _pumpService.Configure(PodAddress, PodIdentifier);
    }

    private async Task ScanAsync()
    {
        if (IsScanning)
        {
            return;
        }

        IsScanning = true;
        ScanStatus = "Scanning for nearby Dash pods…";

        try
        {
            var allowed = await _permissionService.EnsureBluetoothPermissionsAsync();
            if (!allowed)
            {
                ScanStatus = "Bluetooth permissions are required to scan.";
                return;
            }

            var devices = await _bluetoothDeviceScanner.ScanAsync();
            NearbyDevices.Clear();
            foreach (var device in devices.OrderBy(d => d.Name))
            {
                NearbyDevices.Add(device);
            }

            ScanStatus = NearbyDevices.Count == 0
                ? "No nearby devices were detected. Make sure your pod is advertising."
                : "Select a device to use its address.";
        }
        catch (Exception ex)
        {
            ScanStatus = $"Scan failed: {ex.Message}";
        }
        finally
        {
            IsScanning = false;
        }
    }

    private void SelectDevice(BluetoothDeviceInfo? device)
    {
        if (device == null)
        {
            return;
        }

        PodAddress = device.Address;
        ScanStatus = $"Selected {device.Name} ({device.Address}).";
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
        {
            return;
        }

        backingStore = value;
        OnPropertyChanged(propertyName);
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
