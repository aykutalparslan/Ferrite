using Ferrite.Data;

namespace Ferrite.Services;

public interface IAccountService
{
    public Task<bool> RegisterDevice(DeviceInfo deviceInfo);
    public Task<bool> UnregisterDevice(long authKeyId, string token, ICollection<long> otherUserIds);
}