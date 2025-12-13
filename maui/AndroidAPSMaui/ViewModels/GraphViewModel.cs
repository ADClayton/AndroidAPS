using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AndroidAPSMaui.Data;
using AndroidAPSMaui.Services;
using Microsoft.Maui.Storage;

namespace AndroidAPSMaui.ViewModels;

public class GraphViewModel : INotifyPropertyChanged
{
    private readonly BgReadingStore _bgReadingStore;
    private readonly PumpEventStore _eventStore;
    private readonly DashPumpService _pumpService;
    private readonly PermissionService _permissionService;
    private bool _isReadingPump;
    private string _status = "Not connected";
    private string _currentGlucose = "—";
    private string _glucoseTimestamp = "Waiting for glucose data";

    public ObservableCollection<BgReading> Readings { get; } = new();
    public ObservableCollection<PumpEvent> PumpEvents { get; } = new();

    public ICommand ReadPumpCommand { get; }

    public string PumpStatus
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public bool IsReadingPump
    {
        get => _isReadingPump;
        set => SetProperty(ref _isReadingPump, value);
    }

    public string CurrentGlucose
    {
        get => _currentGlucose;
        set => SetProperty(ref _currentGlucose, value);
    }

    public string GlucoseTimestamp
    {
        get => _glucoseTimestamp;
        set => SetProperty(ref _glucoseTimestamp, value);
    }

    public GraphViewModel(BgReadingStore bgReadingStore, PumpEventStore eventStore, DashPumpService pumpService, PermissionService permissionService)
    {
        _bgReadingStore = bgReadingStore;
        _eventStore = eventStore;
        _pumpService = pumpService;
        _permissionService = permissionService;
        ReadPumpCommand = new Command(async () => await ReadPumpAsync());

        ConfigurePumpFromPreferences();
        _bgReadingStore.ReadingsChanged += (_, _) => RefreshReadings();
        _eventStore.EventsChanged += (_, _) => RefreshEvents();

        RefreshReadings();
        RefreshEvents();
    }

    private void RefreshReadings()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Readings.Clear();
            foreach (var reading in _bgReadingStore.Readings)
            {
                Readings.Add(reading);
            }

            var latest = Readings.LastOrDefault();
            if (latest != null)
            {
                CurrentGlucose = Math.Round(latest.Value).ToString();
                GlucoseTimestamp = $"Updated {latest.Timestamp:t}";
            }
            else
            {
                CurrentGlucose = "—";
                GlucoseTimestamp = "Waiting for glucose data";
            }
        });
    }

    private void RefreshEvents()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            PumpEvents.Clear();
            foreach (var pumpEvent in _eventStore.Events)
            {
                PumpEvents.Add(pumpEvent);
            }
        });
    }

    private async Task ReadPumpAsync()
    {
        if (IsReadingPump)
        {
            return;
        }

        if (!TryConfigureFromPreferences(out var configurationMessage))
        {
            PumpStatus = configurationMessage;
            return;
        }

        var permissionsGranted = await _permissionService.EnsureBluetoothPermissionsAsync();
        if (!permissionsGranted)
        {
            PumpStatus = "Bluetooth permissions are required.";
            return;
        }

        IsReadingPump = true;
        PumpStatus = "Reading pump…";
        try
        {
            var status = await _pumpService.ReadStatusAsync();
            PumpStatus = $"{status.ConnectionState} • {status.RetrievedAt:t}";
        }
        catch (Exception ex)
        {
            PumpStatus = $"Pump read failed: {ex.Message}";
        }
        finally
        {
            IsReadingPump = false;
        }
    }

    private void ConfigurePumpFromPreferences()
    {
        TryConfigureFromPreferences(out _);
    }

    private bool TryConfigureFromPreferences(out string message)
    {
        var address = Preferences.Default.Get("dash_pod_address", string.Empty);
        var identifier = Preferences.Default.Get("dash_pod_identifier", string.Empty);

        if (string.IsNullOrWhiteSpace(address))
        {
            message = "Set the Dash pod Bluetooth address before reading.";
            return false;
        }

        _pumpService.Configure(address, identifier);
        message = string.Empty;
        return true;
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
