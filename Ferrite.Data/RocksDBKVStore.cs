/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using RocksDbSharp;

namespace Ferrite.Data;

public class RocksDBKVStore:IKVStore
{
    private RocksDb? db = null;

    public void Init(string path)
    {
        db = RocksDb.Open(new DbOptions().SetCreateIfMissing(), path);
    }
    public void Put(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value)
    {
        if(db == null)
        {
            throw new Exception("Not initialized! You must first call Init() " +
                "with a valid path to the database directory");
        }
        db.Put(key, value);
    }
    public byte[] Get(ReadOnlySpan<byte> key)
    {
        if (db == null)
        {
            throw new Exception("Not initialized! You must first call Init() " +
                "with a valid path to the database directory");
        }
        return db.Get(key);
    }
    public void Remove(ReadOnlySpan<byte> key)
    {
        if (db == null)
        {
            throw new Exception("Not initialized! You must first call Init() " +
                "with a valid path to the database directory");
        }
        db.Remove(key);
    }
}

