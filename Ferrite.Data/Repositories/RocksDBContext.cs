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

using RocksDbSharp;

namespace Ferrite.Data.Repositories;

//TODO: add transaction support
public class RocksDBContext : IDisposable
{
    private readonly RocksDb _db;
    private readonly ColumnFamilyHandle _cf;
    public RocksDBContext()
    {
        _db = RocksDb.Open( new DbOptions().SetCreateIfMissing(true), "ferrite", new ColumnFamilies());
        _cf = _db.GetDefaultColumnFamily();
    }
    public RocksDBContext(string path)
    {
        _db = RocksDb.Open( new DbOptions().SetCreateIfMissing(true), path, new ColumnFamilies());
        _cf = _db.GetDefaultColumnFamily();
    }
    public void Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
    {
        _db.Put(key, value, _cf);
    }
    public byte[] Get(ReadOnlySpan<byte> key)
    {
        return _db.Get(key, _cf);
    }
    public void Delete(ReadOnlySpan<byte> key)
    {
        _db.Remove(key, _cf);
    }
    public void DeleteWithPrefix(byte[] key)
    {
        _db.RemoveWithPrefix(key, _cf);
    }
    public IEnumerable<byte[]> Iterate(byte[] key)
    {
        var iter = _db.NewIterator(_cf);
        iter.Seek(key);
        while(iter.Valid())
        {
            if (iter.Key().Length < key.Length) yield break;
            if (!key.AsSpan().SequenceEqual(iter.Key().AsSpan(0, key.Length)))
            {
                yield break;
            }
            yield return iter.Value();
            iter.Next();
        }
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}