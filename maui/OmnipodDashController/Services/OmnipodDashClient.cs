using System.Collections.Concurrent;
using OmnipodDashController.Models;

namespace OmnipodDashController.Services;

/// <summary>
/// Lightweight MAUI-friendly service that mirrors the Omnipod dash communication surface
/// area from <c>app.aaps.pump.omnipod.dash.driver.OmnipodDashManager</c>. The underlying
/// packet shapes and activation workflow have been extracted from the Android module so
/// that the MAUI shell can reuse the same steps when a pod has already been started by
/// AndroidAPS.
/// </summary>
public class OmnipodDashClient : IOmnipodDashClient
{
    private const int Nonce = 1229869870; // matches OmnipodDashManagerImpl.NONCE
    private readonly ConcurrentQueue<PodEvent> _pendingEvents = new();
    private string? _podId;
    private string? _pairingPin;

    public event EventHandler<string>? EventReceived;

    public Task ConnectAsync(string podId, string pairingPin)
    {
        _podId = podId?.Trim();
        _pairingPin = string.IsNullOrWhiteSpace(pairingPin) ? null : pairingPin.Trim();

        Emit(new PodEvent.Scanning());
        Emit(new PodEvent.BluetoothConnecting());

        // The Android driver pairs first and then uses a fixed nonce before establishing
        // the session; we mirror the same steps so that the MAUI app can attach to a pod
        // that is already running. Real BLE traffic still needs platform bindings.
        if (!string.IsNullOrEmpty(_podId))
        {
            Emit(new PodEvent.AlreadyPaired());
            Emit(new PodEvent.Paired(_podId));
        }

        Emit(new PodEvent.EstablishingSession());
        Emit(new PodEvent.Connected());
        Emit(new PodEvent.CommandSending($"Handshake nonce {Nonce}"));
        Emit(new PodEvent.CommandSent("Handshake"));

        return Task.CompletedTask;
    }

    public Task RefreshStatusAsync()
    {
        Emit(new PodEvent.CommandSending("GetStatus"));
        Emit(new PodEvent.ResponseReceived("GetStatus", "Awaiting BLE implementation"));
        return Task.CompletedTask;
    }

    public Task SuspendAsync()
    {
        Emit(new PodEvent.CommandSending("SuspendDelivery"));
        Emit(new PodEvent.ResponseReceived("SuspendDelivery", "Awaiting BLE implementation"));
        return Task.CompletedTask;
    }

    private void Emit(PodEvent podEvent)
    {
        var message = podEvent switch
        {
            PodEvent.BluetoothConnected e => $"Connected to {e.BluetoothAddress}",
            PodEvent.AlreadyConnected e => $"Already connected to {e.BluetoothAddress}",
            PodEvent.Paired e => $"Paired with pod {e.UniqueId}",
            PodEvent.CommandSending e => $"Sending: {e.Command}",
            PodEvent.CommandSent e => $"Sent: {e.Command}",
            PodEvent.CommandSendNotConfirmed e => $"Send not confirmed: {e.Command}",
            PodEvent.ResponseReceived e => $"Response for {e.Command}: {e.Response}",
            _ => podEvent.GetType().Name
        };

        EventReceived?.Invoke(this, message);
        _pendingEvents.Enqueue(podEvent);
    }
}
