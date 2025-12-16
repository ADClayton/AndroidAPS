namespace OmnipodDashController.Models;

public abstract record PodEvent
{
    public record AlreadyConnected(string BluetoothAddress) : PodEvent;
    public record AlreadyPaired() : PodEvent;
    public record Scanning() : PodEvent;
    public record BluetoothConnecting() : PodEvent;
    public record BluetoothConnected(string BluetoothAddress) : PodEvent;
    public record Pairing() : PodEvent;
    public record Paired(string UniqueId) : PodEvent;
    public record EstablishingSession() : PodEvent;
    public record Connected() : PodEvent;

    public record CommandSending(string Command) : PodEvent;
    public record CommandSent(string Command) : PodEvent;
    public record CommandSendNotConfirmed(string Command) : PodEvent;
    public record ResponseReceived(string Command, string Response) : PodEvent;
}
