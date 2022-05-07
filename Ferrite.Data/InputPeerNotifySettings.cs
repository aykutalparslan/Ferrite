namespace Ferrite.Data;

public record InputPeerNotifySettings
{
    public bool ShowPreviews { get; init; }
    public bool Silent { get; init; }
    public int MuteUntil { get; init; }
    public string Sound { get; init; } = default!;
}