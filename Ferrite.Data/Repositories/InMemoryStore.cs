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

using System.Threading.Channels;
using NonBlocking;

namespace Ferrite.Data.Repositories;

public class InMemoryStore : IVolatileKVStore
{
    // We will be using Redis unless we have a really small deployment
    // in which case this will just do fine however we should still
    // optimize this in the future
    // TODO: Benchmark and optimize this
    private readonly ConcurrentDictionary<byte[], (byte[], long)> _dictionary = new(new ArrayEqualityComparer());
    private readonly PriorityQueue<EncodedKey, long> _ttlQueue = new PriorityQueue<EncodedKey, long>();
    private readonly Channel<EncodedKey> _ttlChannel = Channel.CreateUnbounded<EncodedKey>();
    private readonly Task? _expire;
    private readonly Task? _addTtl;
    private TableDefinition? _table;

    public InMemoryStore()
    {
        _expire = DoExpire();
        _addTtl = DoAddTtl();
    }

    private async Task DoExpire()
    {
        while (true)
        {
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            _ttlQueue.TryPeek(out var key, out long expiration);
            while (expiration <= now)
            {
                _ttlQueue.TryDequeue(out var discardKey, out var discardPriority);
                _dictionary.TryRemove(key.ArrayValue, out var removed);
                key = _ttlQueue.Peek();
            }
            await Task.Delay(50);
        }
    }
    
    private async Task DoAddTtl()
    {
        while (true)
        {
            var key = await _ttlChannel.Reader.ReadAsync();
            _ttlQueue.Enqueue(key, key.ExpiresAt);
        }
    }

    public void SetSchema(TableDefinition table)
    {
        _table = table;
    }

    public void Put(byte[] value, int ttl, params object[] keys)
    {
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        if (ttl > 0)
        {
            primaryKey.ExpiresAt = DateTimeOffset.Now.ToUnixTimeMilliseconds() + ttl * 1000;
        }
        _dictionary.TryAdd(primaryKey.ArrayValue, (value, primaryKey.ExpiresAt));
        if (ttl > 0)
        {
            _ttlChannel.Writer.WriteAsync(primaryKey);
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
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (value.Item2 > 0 && value.Item2 <= now)
        {
            _dictionary.TryRemove(primaryKey.ArrayValue, out var removed);
        }
        return value.Item1;
    }
}