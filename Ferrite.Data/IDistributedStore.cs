using System;
namespace Ferrite.Data;

public interface IDistributedStore
{
    public Task<byte[]> GetAuthKeyAsync(long authKeyId);
    public Task<bool> PutAuthKeyAsync(long authKeyId, byte[] authKey);
    public Task<byte[]> GetSessionAsync(long sessionId);
    public Task<bool> PutSessionAsync(long sessionId, byte[] sessionData);
    public Task<bool> RemoveSessionAsync(long sessionId);
}

