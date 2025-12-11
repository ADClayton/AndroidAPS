using System.ComponentModel;
using System.Runtime.CompilerServices;
using AndroidAPSMaui.Services;

namespace AndroidAPSMaui.ViewModels;

public class PumpConnectionViewModel : INotifyPropertyChanged
{
    private readonly DashPumpService _pumpService;
    private string? _podAddress;
    private string? _podIdentifier;

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

    public Command SaveCommand { get; }

    public PumpConnectionViewModel(DashPumpService pumpService)
    {
        _pumpService = pumpService;
        _podAddress = Preferences.Default.Get("dash_pod_address", string.Empty);
        _podIdentifier = Preferences.Default.Get("dash_pod_identifier", string.Empty);
        _pumpService.Configure(_podAddress, _podIdentifier);
        SaveCommand = new Command(Save);
    }

    private void Save()
    {
        Preferences.Default.Set("dash_pod_address", PodAddress ?? string.Empty);
        Preferences.Default.Set("dash_pod_identifier", PodIdentifier ?? string.Empty);
        _pumpService.Configure(PodAddress, PodIdentifier);
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
