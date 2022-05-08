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
using Ferrite.Utils;
using MessagePack;
using StackExchange.Redis;

namespace Ferrite.Data;

public class RedisCache: IDistributedCache
{
    private readonly ConnectionMultiplexer redis;
    private readonly IMTProtoTime _time;
    private const int ExpiryMaxSeconds = 3600;
    private const string AuthKeyPrefix = "auth-";
    private const string TempAuthKeyPrefix = "tauth-";
    private const string BoundAuthKeyPrefix = "bauth-";
    private const string BoundTempAuthKeyPrefix = "btauth-";
    private const string BoundTempKeysPrefix = "bkeys-";
    private const string SessionPrefix = "ses-s";
    private const string SessionByAuthKeyPrefix = "sbauth-";
    private const string PhoneCodePrefix = "pcode-";
    private const string AuthSessionPrefix = "asess-";
    private const string ServerSaltPrefix = "salt-";
    private const string LoginTokenPrefix = "ltoken-";
    private const string UserStatusPrefix = "ustat-";

    public RedisCache(string config, IMTProtoTime time)
    {
        redis = ConnectionMultiplexer.Connect(config);
        _time = time;
    }

    public async Task<bool> DeleteAuthKeyAsync(long authKeyId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(authKeyId);
        key = key.Prepend(AuthKeyPrefix);
        return await db.KeyDeleteAsync(key);
    }

    public async Task<bool> DeletePhoneCodeAsync(string phoneNumber, string phoneCodeHash)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = phoneNumber + phoneCodeHash;
        key = key.Prepend(PhoneCodePrefix);
        return await db.KeyDeleteAsync(key);
    }

    public async Task<bool> DeleteSessionForAuthKeyAsync(long authKeyId, long sessionId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(authKeyId);
        key = key.Prepend(SessionByAuthKeyPrefix);
        return await db.SortedSetRemoveAsync(key, sessionId);
    }

    public async Task<bool> DeleteTempAuthKeysAsync(long authKeyId, ICollection<long> exceptIds)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BoundTempKeysPrefix;
        key = key.Append(BitConverter.GetBytes(authKeyId));
        await db.SortedSetRemoveRangeByScoreAsync(key, 0, 
            (DateTimeOffset.Now - new TimeSpan(0,0,ExpiryMaxSeconds)).ToUnixTimeMilliseconds());
        var result = await db.SortedSetRangeByScoreAsync(key);
        List<RedisValue> toBeDeleted = new();
        foreach (var v in result)
        {
            if (!exceptIds.Contains((long)v))
            {
                RedisKey tkey = BitConverter.GetBytes((long)v);
                tkey = tkey.Prepend(TempAuthKeyPrefix);
                _ = db.KeyDeleteAsync(tkey);
                toBeDeleted.Add(v);
            }
        }
        _ = db.SortedSetRemoveAsync(key, toBeDeleted.ToArray());
        return true;
    }

    public async Task<byte[]> GetAuthKeyAsync(long authKeyId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(authKeyId);
        key = key.Prepend(AuthKeyPrefix);
        return await db.StringGetAsync(key);
    }

    public byte[] GetAuthKey(long authKeyId)
    {
        IDatabase db = redis.GetDatabase();
        RedisKey key = BitConverter.GetBytes(authKeyId);
        key = key.Prepend(AuthKeyPrefix);
        return db.StringGet(key);
    }

    public async Task<byte[]> GetAuthKeySessionAsync(byte[] nonce)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = nonce;
        key = key.Prepend(AuthSessionPrefix);
        return await db.StringGetAsync(key);
    }

    public async Task<long?> GetBoundAuthKeyAsync(long tempAuthKeyId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(tempAuthKeyId);
        key = key.Prepend(BoundTempAuthKeyPrefix);
        var bound =  (long)await db.StringGetAsync(key);
        key = BitConverter.GetBytes(bound);
        key = key.Prepend(BoundAuthKeyPrefix);
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
        key = key.Prepend(LoginTokenPrefix);
        var result =  await db.StringGetAsync(key);
        if(result == RedisValue.Null)
        {
            return null;
        }
        var login = MessagePackSerializer.Deserialize<LoginViaQR>(result);
        return login;
    }

    public async Task<bool> PutUserStatusAsync(long userId, bool status)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(userId);
        key = key.Prepend(UserStatusPrefix);
        bool success = await db.HashSetAsync(key, "status", status);
        if (status)
        {
            success = await db.HashSetAsync(key, "wasOnline", _time.GetUnixTimeInSeconds());
        }
        
        return success;
    }

    public async Task<(int wasOnline, bool online)> GetUserStatusAsync(long userId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(userId);
        key = key.Prepend(UserStatusPrefix);
        var entries = await db.HashGetAllAsync(key);
        int wasOnline = 0;
        bool status = false;
        foreach (var entry in entries)
        {
            if (entry.Name == "wasOnline")
            {
                wasOnline = (int)entry.Value;
            } else if (entry.Name == "status")
            {
                status = (bool)entry.Value;
            }
        }

        return (wasOnline, status);
    }

    public async Task<string> GetPhoneCodeAsync(string phoneNumber, string phoneCodeHash)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = phoneNumber + phoneCodeHash;
        key = key.Prepend(PhoneCodePrefix);     
        return await db.StringGetAsync(key);
    }

    public async Task<long> GetServerSaltValidityAsync(long authKeyId, long serverSalt)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = ServerSaltPrefix;
        key = key.Append(BitConverter.GetBytes(authKeyId));
        key = key.Append(BitConverter.GetBytes(serverSalt));
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
        key = key.Prepend(SessionPrefix);
        return await db.StringGetAsync(key);
    }

    public async Task<ICollection<long>> GetSessionsByAuthKeyAsync(long authKeyId, TimeSpan expire)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(authKeyId);
        key = key.Prepend(SessionByAuthKeyPrefix);
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
        key = key.Prepend(TempAuthKeyPrefix);
        return await db.StringGetAsync(key);
    }

    public async Task<bool> PutAuthKeyAsync(long authKeyId, byte[] authKey)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(authKeyId);
        key = key.Prepend(AuthKeyPrefix);
        return await db.StringSetAsync(key, (RedisValue)authKey);
    }

    public async Task<bool> PutAuthKeySessionAsync(byte[] nonce, byte[] sessionData)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = nonce;
        key = key.Prepend(AuthSessionPrefix);
        return await db.StringSetAsync(key, (RedisValue)sessionData, new TimeSpan(0,0,600));
    }

    public async Task<bool> PutBoundAuthKeyAsync(long tempAuthKeyId, long authKeyId, TimeSpan expiresIn)
    {
        if (expiresIn.TotalSeconds > ExpiryMaxSeconds)
        {
            expiresIn = new TimeSpan(0, 0, ExpiryMaxSeconds);
        }
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(tempAuthKeyId);
        key = key.Prepend(BoundTempAuthKeyPrefix);
        await db.StringSetAsync(key, authKeyId, expiresIn);
        key = BitConverter.GetBytes(authKeyId);
        key = key.Prepend(BoundAuthKeyPrefix);
        await db.StringSetAsync(key, tempAuthKeyId, expiresIn);
        key = BoundTempKeysPrefix;
        key = key.Append(BitConverter.GetBytes(authKeyId));
        await db.SortedSetRemoveRangeByScoreAsync(key, 0, 
            (DateTimeOffset.Now - new TimeSpan(0,0,ExpiryMaxSeconds)).ToUnixTimeMilliseconds());
        await db.SortedSetAddAsync(key, tempAuthKeyId, DateTimeOffset.Now.ToUnixTimeMilliseconds());
        return true;
    }

    public async Task<bool> PutLoginTokenAsync(LoginViaQR login, TimeSpan expiresIn)
    {
        if (expiresIn.TotalSeconds > ExpiryMaxSeconds)
        {
            expiresIn = new TimeSpan(0, 0, ExpiryMaxSeconds);
        }
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = login.Token;
        key = key.Prepend(LoginTokenPrefix);
        var loginBytes = MessagePackSerializer.Serialize<LoginViaQR>(login);
        return await db.StringSetAsync(key, loginBytes, expiresIn);
    }

    public async Task<bool> PutPhoneCodeAsync(string phoneNumber, string phoneCodeHash, string phoneCode, TimeSpan expiresIn)
    {
        if (expiresIn.TotalSeconds > ExpiryMaxSeconds)
        {
            expiresIn = new TimeSpan(0, 0, ExpiryMaxSeconds);
        }
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = phoneNumber + phoneCodeHash;
        key = key.Prepend(PhoneCodePrefix);
        return await db.StringSetAsync(key, phoneCode, expiresIn);
    }

    public async Task<bool> PutServerSaltAsync(long authKeyId, long serverSalt, long validSince, TimeSpan expiresIn)
    { 
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = ServerSaltPrefix;
        key = key.Append(BitConverter.GetBytes(authKeyId));
        key = key.Append(BitConverter.GetBytes(serverSalt));
        return await db.StringSetAsync(key, BitConverter.GetBytes(validSince), expiresIn);
    }

    public async Task<bool> PutSessionAsync(long sessionId, byte[] sessionData, TimeSpan expire)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(sessionId);
        key = key.Prepend(SessionPrefix);
        return await db.StringSetAsync(key, (RedisValue)sessionData, expire);
    }

    public async Task<bool> PutSessionForAuthKeyAsync(long authKeyId, long sessionId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(authKeyId);
        key = key.Prepend(SessionByAuthKeyPrefix);
        return await db.SortedSetAddAsync(key, (RedisValue)sessionId, DateTimeOffset.Now.ToUnixTimeMilliseconds());
    }

    public async Task<bool> PutTempAuthKeyAsync(long tempAuthKeyId, byte[] tempAuthKey, TimeSpan expiresIn)
    {
        if (expiresIn.TotalSeconds > ExpiryMaxSeconds)
        {
            expiresIn = new TimeSpan(0, 0, ExpiryMaxSeconds);
        }
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(tempAuthKeyId);
        key = key.Prepend(TempAuthKeyPrefix);
        return await db.StringSetAsync(key, (RedisValue)tempAuthKey);
    }

    public async Task<bool> RemoveAuthKeySessionAsync(byte[] nonce)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = nonce;
        key = key.Prepend(AuthSessionPrefix);
        return await db.KeyDeleteAsync(key);
    }

    public async Task<bool> DeleteSessionAsync(long sessionId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(sessionId);
        key = key.Prepend(SessionPrefix);
        return await db.KeyDeleteAsync(key);
    }

    public async Task<bool> SetSessionTTLAsync(long sessionId, TimeSpan expire)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(sessionId);
        key = key.Prepend(SessionPrefix);
        return await db.KeyExpireAsync(key, expire);
    }
}

