namespace Ferrite.Data;

public record PeerNotifySettings
{
    public bool ShowPreviews { get; init; }
    public bool Silent { get; init; }
    public int MuteUntil { get; init; }
    public NotifySoundType NotifySoundType { get; init; }
    public string Title { get; init; } = default!;
    public string Data { get; init; } = default!;
    public long Id { get; init; }
    public DeviceType DeviceType { get; init; }
}