/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
namespace Ferrite.Data;

public class PersistentDataStore : IPersistentStore
{
    private IKVStore kVStore;
    public PersistentDataStore(IKVStore store)
    {
        kVStore = store;
        store.Init("data.rocks");
    }

    public byte[] GetAuthKey(ReadOnlySpan<byte> authKeyId)
    {
        return kVStore.Get(authKeyId);
    }

    public void SaveAuthKey(ReadOnlySpan<byte> authKeyId, ReadOnlySpan<byte> authKey)
    {
        kVStore.Put(authKeyId, authKey);
    }
}


