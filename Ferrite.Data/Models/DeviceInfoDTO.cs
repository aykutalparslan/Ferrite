using MessagePack;

namespace Ferrite.Data;

[MessagePackObject(true)]
public record DeviceInfoDTO
{
    public long AuthKeyId { get; init; }
    public bool NoMuted { get; init; }
    public int TokenType { get; init; }
    public string Token { get; init; } = default!;
    public bool AppSandbox { get; init; }
    public byte[] Secret { get; init; } = default!;
    public ICollection<long> OtherUserIds { get; init; } = default!;
}