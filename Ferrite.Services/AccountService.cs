using Ferrite.Data;

namespace Ferrite.Services;

public class AccountService : IAccountService
{
    private readonly IDistributedCache _cache;
    private readonly IPersistentStore _store;
    public AccountService(IDistributedCache cache, IPersistentStore store)
    {
        _cache = cache;
        _store = store;
    }
    public async Task<bool> RegisterDevice(DeviceInfo deviceInfo)
    {
        return await _store.SaveDeviceInfoAsync(deviceInfo);
    }
}