// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using StackExchange.Redis;

namespace Ferrite.Data.Repositories;

public class RedisDataStore : IVolatileKVStore
{
    private TableDefinition? _table;
    private readonly ConnectionMultiplexer _redis;

    public RedisDataStore(string config)
    {
        _redis = ConnectionMultiplexer.Connect(config);
    }
    public void SetSchema(TableDefinition table)
    {
        _table = table;
    }

    public void Put(byte[] value, TimeSpan? ttl = null, params object[] keys)
    {
        IDatabase db = _redis.GetDatabase();
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        RedisKey key = primaryKey.ArrayValue;
        db.StringSet(key, (RedisValue)value, ttl);
    }

    public void UpdateTtl(TimeSpan? ttl = null, params object[] keys)
    {
        IDatabase db = _redis.GetDatabase();
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        RedisKey key = primaryKey.ArrayValue;
        db.KeyExpire(key, ttl);
    }

    public bool ListAdd(long score, byte[] value, TimeSpan? ttl = null, params object[] keys)
    {
        IDatabase db = _redis.GetDatabase();
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        RedisKey key = primaryKey.ArrayValue;
        db.SortedSetAdd(key, (RedisValue)value, score);
        if (ttl != null)
        {
            db.KeyExpire(key, ttl);
        }

        return true;
    }

    public bool ListDelete(byte[] value, params object[] keys)
    {
        IDatabase db = _redis.GetDatabase();
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        RedisKey key = primaryKey.ArrayValue;
        return db.SortedSetRemove(key, value);
    }

    public bool ListDeleteByScore(long score, params object[] keys)
    {
        IDatabase db = _redis.GetDatabase();
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        RedisKey key = primaryKey.ArrayValue;
        db.SortedSetRemoveRangeByScore(key, 0, score);
        return true;
    }

    public IList<byte[]> ListGet(params object[] keys)
    {
        IDatabase db = _redis.GetDatabase();
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        RedisKey key = primaryKey.ArrayValue;
        var result = db.SortedSetRangeByScore(key);
        var list = Array.ConvertAll<RedisValue, byte[]>(result, item => (byte[])item);
        return list;
    }

    public void Delete(params object[] keys)
    {
        IDatabase db = _redis.GetDatabase();
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        RedisKey key = primaryKey.ArrayValue;
        db.KeyDelete(key);
    }

    public bool Exists(params object[] keys)
    {
        IDatabase db = _redis.GetDatabase();
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        RedisKey key = primaryKey.ArrayValue;
        return db.KeyExists(key);
    }

    public byte[]? Get(params object[] keys)
    {
        IDatabase db = _redis.GetDatabase();
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        RedisKey key = primaryKey.ArrayValue;
        return db.StringGet(key);
    }

    public async ValueTask<byte[]?> GetAsync(params object[] keys)
    {
        object _asyncState = new object();
        IDatabase db = _redis.GetDatabase(asyncState: _asyncState);
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        RedisKey key = primaryKey.ArrayValue;
        return await db.StringGetAsync(key);
    }
}