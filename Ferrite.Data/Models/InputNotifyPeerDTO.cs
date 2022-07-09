namespace Ferrite.Data;

public record InputNotifyPeerDTO
{
    public InputNotifyPeerType NotifyPeerType { get; init; }
    public InputPeerDTO Peer { get; init; } = default!;
}