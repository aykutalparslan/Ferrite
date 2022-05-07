using Ferrite.Data;

namespace Ferrite.Services;

public interface IAccountService
{
    public Task<bool> RegisterDevice(DeviceInfo deviceInfo);
    public Task<bool> UnregisterDevice(long authKeyId, string token, ICollection<long> otherUserIds);
    public Task<bool> UpdateNotifySettings(long authKeyId, InputNotifyPeer peer, InputPeerNotifySettings settings);
    public Task<InputPeerNotifySettings> GetNotifySettings(long authKeyId, InputNotifyPeer peer);
    public Task<bool> ResetNotifySettings(long authKeyId);
}