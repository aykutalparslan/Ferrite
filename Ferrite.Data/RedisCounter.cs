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

    public async ValueTask<long> Get()
    {
        object _asyncState = new object();
        IDatabase db = _redis.GetDatabase(asyncState: _asyncState);
        return (long)await db.StringGetAsync(_name);
    }

    public async ValueTask<long> IncrementAndGet()
    {
        object _asyncState = new object();
        IDatabase db = _redis.GetDatabase(asyncState: _asyncState);
        return await db.StringIncrementAsync(_name);
    }

    public async ValueTask<long> IncrementByAndGet(long inc)
    {
        object _asyncState = new object();
        IDatabase db = _redis.GetDatabase(asyncState: _asyncState);
        return await db.StringIncrementAsync(_name, inc);
    }

    public async ValueTask<long> IncrementTo(long value)
    {
        object _asyncState = new object();
        IDatabase db = _redis.GetDatabase(asyncState: _asyncState);
        var oldValue = (long)await db.StringGetAsync(_name);
        if (oldValue > value)
        {
            return oldValue;
        }
        var tran = db.CreateTransaction();
        tran.AddCondition(Condition.StringEqual(_name, oldValue));
        await tran.StringSetAsync(_name, value);
        await tran.ExecuteAsync();
        return value;
    }

    public async ValueTask DisposeAsync()
    {
        await _redis.DisposeAsync();
    }
}

