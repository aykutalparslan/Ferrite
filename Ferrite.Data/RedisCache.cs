/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Buffers.Binary;
using Ferrite.Data.Auth;
using MessagePack;
using StackExchange.Redis;

namespace Ferrite.Data;

public class RedisCache: IDistributedCache
{
    private readonly ConnectionMultiplexer redis;
    private readonly byte[] AuthKeyPrefix = new byte[] { (byte)'A', (byte)'U', (byte)'T', (byte)'H', (byte)'-', (byte)'-' };
    private readonly byte[] TempAuthKeyPrefix = new byte[] { (byte)'T', (byte)'A', (byte)'U', (byte)'T', (byte)'-', (byte)'-' };
    private readonly byte[] BoundAuthKeyPrefix = new byte[] { (byte)'B', (byte)'A', (byte)'U', (byte)'T', (byte)'-', (byte)'-' };
    private readonly byte[] BoundTempAuthKeyPrefix = new byte[] { (byte)'B', (byte)'T', (byte)'A', (byte)'U', (byte)'-', (byte)'-' };
    private readonly byte[] SessionPrefix = new byte[] { (byte)'S', (byte)'E', (byte)'S', (byte)'S', (byte)'-', (byte)'-' };
    private readonly byte[] SessionByAuthKeyPrefix = new byte[] { (byte)'S', (byte)'A', (byte)'T', (byte)'K', (byte)'-', (byte)'-' };
    private readonly byte[] PhoneCodePrefix = new byte[] { (byte)'P', (byte)'C', (byte)'D', (byte)'E', (byte)'-', (byte)'-' };
    private readonly byte[] AuthSessionPrefix = new byte[] { (byte)'A', (byte)'K', (byte)'C', (byte)'R', (byte)'-', (byte)'-' };
    private readonly byte[] ServerSaltPrefix = new byte[] { (byte)'S', (byte)'A', (byte)'L', (byte)'T', (byte)'-', (byte)'-' };
    private readonly byte[] LoginTokenPrefix = new byte[] { (byte)'L', (byte)'T', (byte)'K', (byte)'N', (byte)'-', (byte)'-' };

    public RedisCache(string config)
    {
        redis = ConnectionMultiplexer.Connect(config);
    }

    public async Task<bool> DeleteAuthKeyAsync(long authKeyId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(authKeyId);
        key.Prepend(AuthKeyPrefix);
        return await db.KeyDeleteAsync(key);
    }

    public async Task<bool> DeletePhoneCodeAsync(string phoneNumber, string phoneCodeHash)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = phoneNumber + phoneCodeHash;
        key.Prepend(PhoneCodePrefix);
        return await db.KeyDeleteAsync(key);
    }

    public async Task<bool> DeleteSessionForAuthKeyAsync(long authKeyId, long sessionId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(authKeyId);
        key.Prepend(SessionByAuthKeyPrefix);
        return await db.SortedSetRemoveAsync(key, sessionId);
    }

    public Task<bool> DeleteTempAuthKeyAsync(long tempAuthKeyId)
    {
        throw new NotImplementedException();
    }

    public async Task<byte[]> GetAuthKeyAsync(long authKeyId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(authKeyId);
        key.Prepend(AuthKeyPrefix);
        return await db.StringGetAsync(key);
    }

    public async Task<byte[]> GetAuthKeySessionAsync(byte[] nonce)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = nonce;
        key.Prepend(AuthSessionPrefix);
        return await db.StringGetAsync(key);
    }

    public async Task<long?> GetBoundAuthKeyAsync(long tempAuthKeyId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(tempAuthKeyId);
        key.Prepend(BoundTempAuthKeyPrefix);
        var bound =  (long)await db.StringGetAsync(key);
        key = BitConverter.GetBytes(bound);
        key.Prepend(BoundAuthKeyPrefix);
        var temp = (long)await db.StringGetAsync(key);
        if(temp == tempAuthKeyId)
        {
            return bound;
        }
        return null;
    }

    public IAtomicCounter GetCounter(string name)
    {
        return new RedisCounter(redis, name);
    }

    public async Task<LoginViaQR?> GetLoginTokenAsync(byte[] token)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = token;
        key.Prepend(LoginTokenPrefix);
        var result =  await db.StringGetAsync(key);
        if(result == RedisValue.Null)
        {
            return null;
        }
        var login = MessagePackSerializer.Deserialize<LoginViaQR>(result);
        return login;
    }

    public async Task<string> GetPhoneCodeAsync(string phoneNumber, string phoneCodeHash)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = phoneNumber + phoneCodeHash;
        key.Prepend(PhoneCodePrefix);     
        return await db.StringGetAsync(key);
    }

    public async Task<long> GetServerSaltValidityAsync(long authKeyId, long serverSalt)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = ServerSaltPrefix;
        key.Append(BitConverter.GetBytes(authKeyId));
        key.Append(BitConverter.GetBytes(serverSalt));
        var val = (byte[])await db.StringGetAsync(key);
        if(val == null)
        {
            return 0;
        }
        return BitConverter.ToInt64(val);
    }

    public async Task<byte[]> GetSessionAsync(long sessionId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(sessionId);
        key.Prepend(SessionPrefix);
        return await db.StringGetAsync(key);
    }

    public async Task<ICollection<long>> GetSessionsByAuthKeyAsync(long authKeyId, TimeSpan expire)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(authKeyId);
        key.Prepend(SessionByAuthKeyPrefix);
        await db.SortedSetRemoveRangeByScoreAsync(key, 0, (DateTimeOffset.Now - expire).ToUnixTimeMilliseconds());
        var result = await db.SortedSetRangeByScoreAsync(key);
        var resultLong = Array.ConvertAll<RedisValue, long>(result, item => (long)item);
        return resultLong;
    }

    public async Task<byte[]?> GetTempAuthKeyAsync(long tempAuthKeyId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(tempAuthKeyId);
        key.Prepend(TempAuthKeyPrefix);
        return await db.StringGetAsync(key);
    }

    public async Task<bool> PutAuthKeyAsync(long authKeyId, byte[] authKey)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(authKeyId);
        key.Prepend(AuthKeyPrefix);
        return await db.StringSetAsync(key, (RedisValue)authKey);
    }

    public async Task<bool> PutAuthKeySessionAsync(byte[] nonce, byte[] sessionData)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = nonce;
        key.Prepend(AuthSessionPrefix);
        return await db.StringSetAsync(key, (RedisValue)sessionData, new TimeSpan(0,0,600));
    }

    public async Task<bool> PutBoundAuthKeyAsync(long tempAuthKeyId, long authKeyId, TimeSpan expiresIn)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(tempAuthKeyId);
        key.Prepend(BoundTempAuthKeyPrefix);
        await db.StringSetAsync(key, authKeyId, expiresIn);
        key = BitConverter.GetBytes(authKeyId);
        key.Prepend(BoundAuthKeyPrefix);
        await db.StringSetAsync(key, tempAuthKeyId, expiresIn);
        return true;
    }

    public async Task<bool> PutLoginTokenAsync(LoginViaQR login, TimeSpan expiresIn)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = login.Token;
        key.Prepend(LoginTokenPrefix);
        var loginBytes = MessagePackSerializer.Serialize<LoginViaQR>(login);
        return await db.StringSetAsync(key, loginBytes, expiresIn);
    }

    public async Task<bool> PutPhoneCodeAsync(string phoneNumber, string phoneCodeHash, string phoneCode, TimeSpan expiresIn)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = phoneNumber + phoneCodeHash;
        key.Prepend(PhoneCodePrefix);
        return await db.StringSetAsync(key, phoneCode, expiresIn);
    }

    public async Task<bool> PutServerSaltAsync(long authKeyId, long serverSalt, long validSince, TimeSpan expiresIn)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = ServerSaltPrefix;
        key.Append(BitConverter.GetBytes(authKeyId));
        key.Append(BitConverter.GetBytes(serverSalt));
        return await db.StringSetAsync(key, BitConverter.GetBytes(validSince), expiresIn);
    }

    public async Task<bool> PutSessionAsync(long sessionId, byte[] sessionData, TimeSpan expire)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(sessionId);
        key.Prepend(SessionPrefix);
        return await db.StringSetAsync(key, (RedisValue)sessionData, expire);
    }

    public async Task<bool> PutSessionForAuthKeyAsync(long authKeyId, long sessionId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(authKeyId);
        key.Prepend(SessionByAuthKeyPrefix);
        return await db.SortedSetAddAsync(key, (RedisValue)sessionId, DateTimeOffset.Now.ToUnixTimeMilliseconds());
    }

    public async Task<bool> PutTempAuthKeyAsync(long tempAuthKeyId, byte[] tempAuthKey, TimeSpan expiresIn)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(tempAuthKeyId);
        key.Prepend(TempAuthKeyPrefix);
        return await db.StringSetAsync(key, (RedisValue)tempAuthKey);
    }

    public async Task<bool> RemoveAuthKeySessionAsync(byte[] nonce)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = nonce;
        key.Prepend(AuthSessionPrefix);
        return await db.KeyDeleteAsync(key);
    }

    public async Task<bool> DeleteSessionAsync(long sessionId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(sessionId);
        key.Prepend(SessionPrefix);
        return await db.KeyDeleteAsync(key);
    }

    public async Task<bool> SetSessionTTLAsync(long sessionId, TimeSpan expire)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(sessionId);
        key.Prepend(SessionPrefix);
        return await db.KeyExpireAsync(key, expire);
    }
}

