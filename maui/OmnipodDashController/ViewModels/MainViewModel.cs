using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using OmnipodDashController.Services;

namespace OmnipodDashController.ViewModels;

public class MainViewModel : BindableObject
{
    private readonly IOmnipodDashClient _client;
    private string _podId = string.Empty;
    private string _pairingPin = string.Empty;
    private string _basalProgram = string.Empty;

    public MainViewModel(IOmnipodDashClient client)
    {
        _client = client;
        Events = new ObservableCollection<string>();

        ConnectCommand = new Command(async () => await ConnectAsync());
        StatusCommand = new Command(async () => await GetStatusAsync());
        SuspendCommand = new Command(async () => await SuspendAsync());

        _client.EventReceived += (_, e) => MainThread.BeginInvokeOnMainThread(() => Events.Insert(0, e));
    }

    public string PodId
    {
        get => _podId;
        set { _podId = value; OnPropertyChanged(); }
    }

    public string PairingPin
    {
        get => _pairingPin;
        set { _pairingPin = value; OnPropertyChanged(); }
    }

    public string BasalProgram
    {
        get => _basalProgram;
        set { _basalProgram = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> Events { get; }

    public ICommand ConnectCommand { get; }

    public ICommand StatusCommand { get; }

    public ICommand SuspendCommand { get; }

    private async Task ConnectAsync()
    {
        Events.Insert(0, "Connecting to pod...");
        await _client.ConnectAsync(PodId, PairingPin);
    }

    private async Task GetStatusAsync()
    {
        await _client.RefreshStatusAsync();
    }

    private async Task SuspendAsync()
    {
        await _client.SuspendAsync();
    }
}
