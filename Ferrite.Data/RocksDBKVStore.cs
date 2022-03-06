/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
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

