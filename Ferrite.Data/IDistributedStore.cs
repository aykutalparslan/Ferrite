using System;
namespace Ferrite.Data;

public interface IDistributedStore
{
    public Task<byte[]> GetAuthKeyAsync(long authKeyId);
    public Task<bool> PutAuthKeyAsync(long authKeyId, byte[] authKey);
    public Task<byte[]> GetSessionAsync(long sessionId);
    public Task<bool> PutSessionAsync(long sessionId, byte[] sessionData);
    public Task<byte[]> GetAuthKeySessionAsync(byte[] nonce);
    public Task<bool> PutAuthKeySessionAsync(byte[] nonce, byte[] sessionData);
    public Task<bool> RemoveSessionAsync(long sessionId);
    public Task<byte[]> GetPhoneCodeAsync(string phoneNumber, string phoneCodeHash);
    public Task<bool> PutPhoneCodeAsync(string phoneNumber, string phoneCodeHash, string phoneCode, TimeSpan expiresIn);
}

