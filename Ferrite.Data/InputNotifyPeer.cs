namespace Ferrite.Data;

public record InputNotifyPeer
{
    public InputNotifyPeerType NotifyPeerType { get; init; }
    public InputPeer Peer { get; init; } = default!;
}