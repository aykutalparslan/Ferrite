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
using Cassandra.DataStax.Graph;
using MessagePack;
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
        (byte[], long) current;
        while (true)
        {
            await Task.Delay(100);
            long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (!_ttlQueue.TryPeek(out var currentKey, out var expiration) ||
                expiration > now)
            {
                continue;
            }
            
            while (expiration <= now)
            {
                _ttlQueue.TryDequeue(out currentKey, out var currentPriority);
                if (currentPriority <= now)
                {
                    _dictionary.TryGetValue(currentKey.ArrayValue, out current);
                    if (current.Item2 <= now)
                    {
                        _dictionary.TryRemove(currentKey.ArrayValue, out current);
                    }
                }
                else
                {
                    _ttlQueue.Enqueue(currentKey, currentPriority);
                }
                if (!_ttlQueue.TryPeek(out currentKey, out expiration))
                {
                    expiration = long.MaxValue;
                }
            }
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
    public void Put(byte[] value, TimeSpan? ttl = null, params object[] keys)
    {
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        if (ttl.HasValue)
        {
            primaryKey.ExpiresAt = DateTimeOffset.Now.ToUnixTimeMilliseconds() + (long)ttl.Value.TotalMilliseconds;
        }
        _dictionary.TryAdd(primaryKey.ArrayValue, (value, primaryKey.ExpiresAt));
        if (ttl.HasValue)
        {
            _ttlChannel.Writer.WriteAsync(primaryKey);
        }
    }

    public void UpdateTtl(TimeSpan? ttl = null, params object[] keys)
    {
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        if (ttl.HasValue)
        {
            primaryKey.ExpiresAt = DateTimeOffset.Now.ToUnixTimeMilliseconds() + (long)ttl.Value.TotalMilliseconds;
        }
        if (_dictionary.ContainsKey(primaryKey.ArrayValue))
        {
            _dictionary.TryGetValue(primaryKey.ArrayValue, out var current);
            _dictionary.TryUpdate(primaryKey.ArrayValue, (current.Item1, primaryKey.ExpiresAt), current);
            if (ttl.HasValue)
            {
                _ttlChannel.Writer.WriteAsync(primaryKey);
            }
        }
    }

    public bool ListAdd(long score, byte[] value, TimeSpan? ttl = null, params object[] keys)
    {
        SortedList<long, byte[]>? list;
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        bool rem = _dictionary.TryRemove(primaryKey.ArrayValue, out var existing);
        (byte[] data, long expiry) = existing;
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (rem && data != null &&
            (expiry <= 0 || expiry <= now))
        {
            try
            {
                list = MessagePackSerializer.Deserialize<SortedList<long, byte[]>>(data);
            }
            catch (MessagePackSerializationException e)
            {
                // we will overwrite the value of the key with a new list in the case of an error
                list = new SortedList<long, byte[]>();
            }
        }
        else
        {
            list = new SortedList<long, byte[]>();
        }
        // keys must be unique in a SortedList
        // and we will be using high resolution timestamps
        // so this ugly hack is okay
        // we are trying to emulate a Redis SortedSet here
        while (list.ContainsKey(score))
        {
            score++;
        }
        list.Add(score, value);
        try
        {
            var serialized = MessagePackSerializer.Serialize(list);
            _dictionary.TryAdd(primaryKey.ArrayValue, (serialized, primaryKey.ExpiresAt));
            if (ttl.HasValue)
            {
                _ttlChannel.Writer.WriteAsync(primaryKey);
            }
        }
        catch (MessagePackSerializationException e)
        {
            return false;
        }

        return true;
    }

    public bool ListDeleteByScore(long score, params object[] keys)
    {
        SortedList<long, byte[]>? list;
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        bool rem = _dictionary.TryRemove(primaryKey.ArrayValue, out var existing);
        (byte[] data, long expiry) = existing;
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (rem && data != null &&
            (expiry <= 0 || expiry <= now))
        {
            try
            {
                list = MessagePackSerializer.Deserialize<SortedList<long, byte[]>>(data);
                foreach (var k in list.Keys)
                {
                    if (k < score)
                    {
                        list.Remove(k);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (MessagePackSerializationException e)
            {
                // nothing to do since this is not a list
                return false;
            }
        }

        return true;
    }

    public IList<byte[]> ListGet(params object[] keys)
    {
        SortedList<long, byte[]>? list;
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        bool rem = _dictionary.TryRemove(primaryKey.ArrayValue, out var existing);
        (byte[] data, long expiry) = existing;
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (rem && data != null &&
            (expiry <= 0 || expiry <= now))
        {
            try
            {
                list = MessagePackSerializer.Deserialize<SortedList<long, byte[]>>(data);
                return list.Values;
            }
            catch (MessagePackSerializationException e)
            {
                return Array.Empty<byte[]>();
            }
        }
        return Array.Empty<byte[]>();
    }

    public void Delete(params object[] keys)
    {
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        _dictionary.TryRemove(primaryKey.ArrayValue, out var removed);
    }

    public bool Exists(params object[] keys)
    {
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        
        if (!_dictionary.TryGetValue(primaryKey.ArrayValue, out var value))
        {
            return false;
        }
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (value.Item2 > 0 && value.Item2 <= now)
        {
            _dictionary.TryRemove(primaryKey.ArrayValue, out var removed);
            return false;
        }

        return true;
    }

    public byte[]? Get(params object[] keys)
    {
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        _dictionary.TryGetValue(primaryKey.ArrayValue, out var value);
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (value.Item2 > 0 && value.Item2 <= now)
        {
            _dictionary.TryRemove(primaryKey.ArrayValue, out var removed);
            return null;
        }
        return value.Item1;
    }

    public ValueTask<byte[]?> GetAsync(params object[] keys)
    {
        var primaryKey = EncodedKey.Create(_table.FullName, keys);
        _dictionary.TryGetValue(primaryKey.ArrayValue, out var value);
        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        if (value.Item2 > 0 && value.Item2 <= now)
        {
            _dictionary.TryRemove(primaryKey.ArrayValue, out var removed);
            return ValueTask.FromResult<byte[]?>(null);
        }
        return ValueTask.FromResult<byte[]?>(value.Item1);;
    }
}