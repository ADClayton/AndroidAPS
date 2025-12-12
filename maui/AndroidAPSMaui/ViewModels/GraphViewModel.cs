using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AndroidAPSMaui.Data;
using AndroidAPSMaui.Services;

namespace AndroidAPSMaui.ViewModels;

public class GraphViewModel : INotifyPropertyChanged
{
    private readonly BgReadingStore _bgReadingStore;
    private readonly PumpEventStore _eventStore;
    private readonly DashPumpService _pumpService;
    private bool _isReadingPump;
    private string _status = "Not connected";

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

    public GraphViewModel(BgReadingStore bgReadingStore, PumpEventStore eventStore, DashPumpService pumpService)
    {
        _bgReadingStore = bgReadingStore;
        _eventStore = eventStore;
        _pumpService = pumpService;
        ReadPumpCommand = new Command(async () => await ReadPumpAsync());

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
