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
using StackExchange.Redis;

namespace Ferrite.Data;

public class RedisDataStore: IDistributedStore
{
    private readonly ConnectionMultiplexer redis;
    private readonly byte[] AuthKeyPrefix = new byte[] { (byte)'A', (byte)'U', (byte)'T', (byte)'H', (byte)'-', (byte)'-' };
    private readonly byte[] SessionPrefix = new byte[] { (byte)'S', (byte)'E', (byte)'S', (byte)'S', (byte)'-', (byte)'-' };
    private readonly byte[] PhoneCodePrefix = new byte[] { (byte)'P', (byte)'C', (byte)'D', (byte)'E', (byte)'-', (byte)'-' };

    public RedisDataStore(string config)
    {
        redis = ConnectionMultiplexer.Connect(config);
    }

    public async Task<byte[]> GetAuthKeyAsync(long authKeyId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(authKeyId);
        key.Prepend(AuthKeyPrefix);
        return await db.StringGetAsync(key);
    }

    public async Task<byte[]> GetPhoneCodeAsync(string phoneNumber, string phoneCodeHash)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = phoneNumber + phoneCodeHash;
        key.Prepend(PhoneCodePrefix);     
        return await db.StringGetAsync(key);
    }

    public async Task<byte[]> GetSessionAsync(long sessionId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(sessionId);
        key.Prepend(SessionPrefix);
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

    public async Task<bool> PutPhoneCodeAsync(string phoneNumber, string phoneCodeHash, string phoneCode, TimeSpan expiresIn)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = phoneNumber + phoneCodeHash;
        key.Prepend(PhoneCodePrefix);
        return await db.StringSetAsync(key, phoneCode, expiresIn);
    }

    public async Task<bool> PutSessionAsync(long sessionId, byte[] sessionData)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(sessionId);
        key.Prepend(SessionPrefix);
        return await db.StringSetAsync(key, (RedisValue)sessionData);
    }

    public async Task<bool> RemoveSessionAsync(long sessionId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        RedisKey key = BitConverter.GetBytes(sessionId);
        key.Prepend(SessionPrefix);
        return await db.KeyDeleteAsync(key);
    }
}

