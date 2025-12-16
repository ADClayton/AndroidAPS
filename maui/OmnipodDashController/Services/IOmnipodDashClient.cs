using OmnipodDashController.Models;

namespace OmnipodDashController.Services;

public interface IOmnipodDashClient
{
    event EventHandler<string>? EventReceived;

    Task ConnectAsync(string podId, string pairingPin);
    Task RefreshStatusAsync();
    Task SuspendAsync();
}
