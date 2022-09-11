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

public static class RocksDbExtensions
{
    private static WriteOptions DefaultWriteOptions { get; } = new WriteOptions();
    public static void RemoveWithPrefix(this RocksDb db, byte[] prefix, ColumnFamilyHandle cf = null, WriteOptions writeOptions = null)
    {
        var end = new byte[prefix.Length];
        prefix.CopyTo(end, 0);
        int idx = end.Length - 1;
        while (++end[idx] == 0 && idx > 0)
        {
            idx--;
        }
        RocksDbSharp.Native.Instance.rocksdb_delete_range_cf(db.Handle,
            (writeOptions ?? DefaultWriteOptions).Handle,
            (cf ?? db.GetDefaultColumnFamily()).Handle,
            prefix, (nuint)prefix.Length,
            end, (nuint)end.Length);
    }
}