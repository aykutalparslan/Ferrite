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

using NonBlocking;

namespace Ferrite.Data.Repositories;

public class InMemoryStore : IVolatileKVStore
{
    // We will be using Redis unless we have a really small deployment
    // in which case this will just do fine however we should still
    // optimize this in the future
    // TODO: Benchmark and optimize this
    private readonly ConcurrentDictionary<byte[], byte[]> _dictionary = new(new ArrayEqualityComparer());
    private readonly PriorityQueue<byte[], long> _expirationQueue = new PriorityQueue<byte[], long>();
    private readonly object _syncRoot = new object();
    private readonly Task? _keyExpiration;
    private TableDefinition? _table;

    public InMemoryStore()
    {
        _keyExpiration = DoExpire();
    }

    private async Task DoExpire()
    {
        while (true)
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            _expirationQueue.TryPeek(out var key, out long expiration);
            while (expiration <= now)
            {
                lock (_syncRoot)
                {
                    _expirationQueue.TryDequeue(out var discardKey, out var discardPriority);
                }
                _dictionary.TryRemove(key, out var removed);
                key = _expirationQueue.Peek();
            }
            await Task.Delay(50);
        }
    }

    public void SetSchema(TableDefinition table)
    {
        _table = table;
    }

    public void Put(byte[] value, int Ttl, params object[] keys)
    {
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        _dictionary.TryAdd(primaryKey.ArrayValue, value);
        long expiration = DateTimeOffset.Now.ToUnixTimeMilliseconds() + Ttl * 1000;
        lock (_syncRoot)
        {
            _expirationQueue.Enqueue(primaryKey.ArrayValue, expiration);
        }
    }

    public void Delete(params object[] keys)
    {
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        _dictionary.TryRemove(primaryKey.ArrayValue, out var removed);
    }

    public bool Exists(params object[] keys)
    {
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        return _dictionary.ContainsKey(primaryKey.ArrayValue);
    }

    public byte[]? Get(params object[] keys)
    {
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        _dictionary.TryGetValue(primaryKey.ArrayValue, out var value);
        return value;
    }
}