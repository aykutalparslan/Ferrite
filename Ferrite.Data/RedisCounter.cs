//
//  Project Ferrite is an Implementation of the Telegram Server API
//  Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using StackExchange.Redis;

namespace Ferrite.Data;

public class RedisCounter : IAtomicCounter
{
    private readonly ConnectionMultiplexer _redis;
    private readonly string _name;
    public RedisCounter(ConnectionMultiplexer redis, string name)
    {
        _redis = redis;
        _name = name;
    }

    public async Task<long> IncrementAndGet()
    {
        object _asyncState = new object();
        IDatabase db = _redis.GetDatabase(asyncState: _asyncState);
        return await db.StringIncrementAsync(_name);
    }

    public async Task<long> IncrementByAndGet(long inc)
    {
        object _asyncState = new object();
        IDatabase db = _redis.GetDatabase(asyncState: _asyncState);
        return await db.StringIncrementAsync(_name, inc);
    }

    public async Task<long> Set(long value)
    {
        object _asyncState = new object();
        IDatabase db = _redis.GetDatabase(asyncState: _asyncState);
        return (long)await db.StringSetAndGetAsync(_name, value);
    }
}

