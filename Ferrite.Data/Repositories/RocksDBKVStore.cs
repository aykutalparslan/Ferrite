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

using DotNext.Collections.Generic;

namespace Ferrite.Data.Repositories;

public class RocksDBKVStore : IKVStore
{
    private TableDefinition _table;
    private readonly RocksDBContext _context;
    private static Type GetManagedType(DataType type) => type switch
    {
        DataType.Bool => typeof(bool),
        DataType.Int => typeof(int),
        DataType.Long => typeof(long),
        DataType.Float => typeof(float),
        DataType.Double => typeof(double),
        DataType.DateTime => typeof(DateTime),
        DataType.String => typeof(string),
        DataType.Bytes => typeof(byte[]),
        _ => typeof(object)
    };
    public RocksDBKVStore(RocksDBContext context)
    {
        _context = context;
    }

    public void SetSchema(TableDefinition table)
    {
        _table = table;
    }

    public bool Put(byte[] data, params object[] keys)
    {
        MemcomparableKey primaryKey = GeneratePrimaryKey(keys);
        PutInternal(data, keys, primaryKey);
        return true;
    }

    private MemcomparableKey GeneratePrimaryKey(object[] keys)
    {
        if (keys.Length != _table.PrimaryKey.Columns.Count)
        {
            throw new Exception("Parameter count mismatch.");
        }

        for (int i = 0; i < keys.Length; i++)
        {
            var col = _table.PrimaryKey.Columns[i];
            if (keys[i].GetType() != GetManagedType(col.Type))
            {
                throw new Exception($"Expected type was {GetManagedType(col.Type)} and " +
                                    $"the parameter was of type {keys[i].GetType()}");
            }
        }

        var primaryKey = MemcomparableKey.Create(_table.FullName, keys);
        return primaryKey;
    }

    private void PutInternal(byte[] data, object[] keys, MemcomparableKey primaryKey)
    {
        _context.Put(primaryKey.Value, data);
        foreach (var sc in _table.SecondaryIndices)
        {
            List<object> secondaryParams = new();
            foreach (var c in sc.Columns)
            {
                secondaryParams.Add(keys[_table.PrimaryKey.GetOrdinal(c.Name)]);
            }

            var secondaryKey = MemcomparableKey.Create(sc.FullName, secondaryParams);
            _context.Put(secondaryKey.Value, primaryKey.Value);
        }
    }

    public bool Delete(params object[] keys)
    {
        var key = MemcomparableKey.Create(_table.FullName, keys);
        DeleteSecondaryIndices(keys, key);
        if (keys.Length == _table.PrimaryKey.Columns.Count) _context.Delete(key.Value);
        else _context.DeleteWithPrefix(key.ArrayValue);
        return true;
    }

    private void DeleteSecondaryIndices(object[] keys, MemcomparableKey key)
    {
        if (keys.Length == _table.PrimaryKey.Columns.Count)
        {
            foreach (var sc in _table.SecondaryIndices)
            {
                List<object> secondaryParams = new();
                foreach (var c in sc.Columns)
                {
                    secondaryParams.Add(keys[_table.PrimaryKey.GetOrdinal(c.Name)]);
                }

                var secondaryKey = MemcomparableKey.Create(sc.FullName, secondaryParams);
                _context.Delete(secondaryKey.Value);
            }
        }
        else if (_table.SecondaryIndices.Count > 0)
        {
            var iter = _context.IterateKeys(key.ArrayValue);
            foreach (var k in iter)
            {
                _context.Delete(k);
                var primaryKey = MemcomparableKey.From(k);
                foreach (var sc in _table.SecondaryIndices)
                {
                    List<object> secondaryParams = new();
                    foreach (var c in sc.Columns)
                    {
                        secondaryParams.Add(primaryKey.GetValue(_table.PrimaryKey, c.Name));
                    }

                    var secondaryKey = MemcomparableKey.Create(sc.FullName, secondaryParams);
                    _context.Delete(secondaryKey.Value);
                }
            }
        }
    }

    public ValueTask<bool> DeleteAsync(params object[] keys)
    {
        var key = MemcomparableKey.Create(_table.FullName, keys);
        if (keys.Length == _table.PrimaryKey.Columns.Count) _context.Delete(key.Value);
        else if(_table.SecondaryIndices.Count == 0) _context.DeleteWithPrefix(key.ArrayValue);
        
        DeleteSecondaryIndices(keys, key);
        return ValueTask.FromResult(true);
    }

    public bool DeleteBySecondaryIndex(string indexName, params object[] keys)
    {
        var sc = _table.SecondaryIndices.FirstOrDefault(x=>x.Name == indexName);
        var secondaryKey = MemcomparableKey.Create(sc.FullName, keys);
        var primaryKey = _context.Get(secondaryKey.Value);
        DeleteSecondaryIndices(keys, MemcomparableKey.From(primaryKey));
        _context.Delete(primaryKey);
        return true;
    }

    public ValueTask<bool> DeleteBySecondaryIndexAsync(string indexName, params object[] keys)
    {
        var sc = _table.SecondaryIndices.FirstOrDefault(x=>x.Name == indexName);
        var secondaryKey = MemcomparableKey.Create(sc.FullName, keys);
        var primaryKey = _context.Get(secondaryKey.Value);
        _context.Delete(primaryKey);
        DeleteSecondaryIndices(keys, MemcomparableKey.From(primaryKey));
        return ValueTask.FromResult(true);
    }

    public byte[]? Get(params object[] keys)
    {
        var key = MemcomparableKey.Create(_table.FullName, keys);
        if (keys.Length == _table.PrimaryKey.Columns.Count)
        {
            return _context.Get(key.Value);
        }
        return _context.Iterate(key.ArrayValue).FirstOrDefault();
    }

    public ValueTask<byte[]?> GetAsync(params object[] keys)
    {
        var key = MemcomparableKey.Create(_table.FullName, keys);
        return new ValueTask<byte[]?>(_context.Get(key.Value));
    }

    public byte[]? GetBySecondaryIndex(string indexName, params object[] keys)
    {
        var sc = _table.SecondaryIndices.FirstOrDefault(x=>x.Name == indexName);
        var secondaryKey = MemcomparableKey.Create(sc.FullName, keys);
        var primaryKey = _context.Get(secondaryKey.Value);
        return _context.Get(primaryKey);
    }

    public ValueTask<byte[]?> GetBySecondaryIndexAsync(string indexName, params object[] keys)
    {
        var sc = _table.SecondaryIndices.FirstOrDefault(x=>x.Name == indexName);
        var secondaryKey = MemcomparableKey.Create(sc.FullName, keys);
        var primaryKey = _context.Get(secondaryKey.Value);
        return new ValueTask<byte[]?>(_context.Get(primaryKey));
    }

    public IEnumerable<byte[]> Iterate(params object[] keys)
    {
        var key = MemcomparableKey.Create(_table.FullName, keys);
        return _context.Iterate(key.ArrayValue);
    }
    
    public async IAsyncEnumerable<byte[]> IterateAsync(params object[] keys)
    {
        var key = MemcomparableKey.Create(_table.FullName, keys);
        var iterator = _context.Iterate(key.ArrayValue);
        foreach (var val in iterator)
        {
            yield return val;
        }
    }

    public IEnumerable<byte[]> IterateBySecondaryIndex(string indexName, params object[] keys)
    {
        var sc = _table.SecondaryIndices.FirstOrDefault(x=>x.Name == indexName);
        var key = MemcomparableKey.Create(sc.FullName, keys);
        var iter = _context.Iterate(key.ArrayValue);
        foreach (var primaryKey in iter)
        {
            yield return _context.Get(primaryKey);
        }
    }

    public async IAsyncEnumerable<byte[]> IterateBySecondaryIndexAsync(string indexName, params object[] keys)
    {
        var sc = _table.SecondaryIndices.FirstOrDefault(x=>x.Name == indexName);
        var key = MemcomparableKey.Create(sc.FullName, keys);
        var iter = _context.Iterate(key.ArrayValue);
        foreach (var primaryKey in iter)
        {
            yield return _context.Get(primaryKey);
        }
    }
}