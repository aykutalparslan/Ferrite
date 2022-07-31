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
public class RocksDBContext
{
    private readonly RocksDb _db;
    public RocksDBContext()
    {
        _db = RocksDb.Open( new DbOptions().SetCreateIfMissing(true), "test" );
    }
    public void Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
    {
        _db.Put(key, value);
    }
    public byte[] Get(ReadOnlySpan<byte> key)
    {
        return _db.Get(key);
    }
    public void Delete(ReadOnlySpan<byte> key)
    {
        _db.Remove(key);
    }
    public IEnumerable<byte[]> Iterate(byte[] key)
    {
        var iter = _db.NewIterator();
        iter.Seek(key);
        while(iter.Valid())
        {
            if (key[^1] != iter.Key()[key.Length-1])
            {
                yield break;
            }
            yield return iter.Value();
            iter.Next();
        }
    }
}