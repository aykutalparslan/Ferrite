using Ferrite.Data;

namespace Ferrite.Services;

public interface IAccountService
{
    public Task<bool> RegisterDevice(DeviceInfo deviceInfo);
}