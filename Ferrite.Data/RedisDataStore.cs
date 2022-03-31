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

    public RedisDataStore(string config)
    {
        redis = ConnectionMultiplexer.Connect(config);
    }

    public async Task<byte[]> GetAuthKeyAsync(long authKeyId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        return await db.StringGetAsync((RedisKey)BitConverter.GetBytes(authKeyId));
    }

    public async Task<byte[]> GetSessionAsync(long sessionId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        return await db.StringGetAsync((RedisKey)BitConverter.GetBytes(sessionId));
    }

    public async Task<bool> PutAuthKeyAsync(long authKeyId, byte[] authKey)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        return await db.StringSetAsync((RedisKey)BitConverter.GetBytes(authKeyId), (RedisValue)authKey);
    }

    public async Task<bool> PutSessionAsync(long sessionId, byte[] sessionData)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        return await db.StringSetAsync((RedisKey)BitConverter.GetBytes(sessionId), (RedisValue)sessionData);
    }

    public async Task<bool> RemoveSessionAsync(long sessionId)
    {
        object _asyncState = new object();
        IDatabase db = redis.GetDatabase(asyncState: _asyncState);
        return await db.KeyDeleteAsync((RedisKey)BitConverter.GetBytes(sessionId));
    }
}

