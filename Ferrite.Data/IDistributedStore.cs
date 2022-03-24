using System;
namespace Ferrite.Data;

public interface IDistributedStore
{
    public byte[] GetAuthKey(byte[] authKeyId);
    public Task<bool> PutAuthKeyAsync(byte[] authKeyId, byte[] authKey);
    public byte[] GetSession(byte[] sessionId);
    public Task<bool> PutSessionAsync(byte[] sessionId, byte[] sessionData);
    public Task<bool> RemoveSessionAsync(byte[] sessionId);
}

