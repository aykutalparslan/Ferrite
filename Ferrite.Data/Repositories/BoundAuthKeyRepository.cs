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

namespace Ferrite.Data.Repositories;

public class BoundAuthKeyRepository : IBoundAuthKeyRepository
{
    private readonly IVolatileKVStore _storeTemp;
    private readonly IVolatileKVStore _storeAuth;
    private readonly IVolatileKVStore _storeBound;
    public BoundAuthKeyRepository(IVolatileKVStore storeTemp, IVolatileKVStore storeAuth,
         IVolatileKVStore storeBound)
    {
        _storeTemp = storeTemp;
        _storeAuth = storeAuth;
        _storeBound = storeBound;
        _storeTemp.SetSchema(new TableDefinition("ferrite", "bound_auth_keys_temp",
            new KeyDefinition("pk",
                new DataColumn { Name = "temp_auth_key_id", Type = DataType.Long })));
        _storeAuth.SetSchema(new TableDefinition("ferrite", "bound_auth_keys_auth",
            new KeyDefinition("pk",
                new DataColumn { Name = "auth_key_id", Type = DataType.Long })));
        _storeBound.SetSchema(new TableDefinition("ferrite", "bound_auth_keys_auth",
            new KeyDefinition("pk",
                new DataColumn { Name = "auth_key_id", Type = DataType.Long })));
    }
    
    public bool PutBoundAuthKey(long tempAuthKeyId, long authKeyId, TimeSpan expiresIn)
    {
        _storeTemp.Put(BitConverter.GetBytes(authKeyId), expiresIn, tempAuthKeyId);
        // each auth key can be bound to a single temp auth key at any given time
        _storeAuth.Put(BitConverter.GetBytes(tempAuthKeyId), expiresIn, authKeyId);
        // we need to retrieve a list of keys that was bound to an auth key in the given timeframe
        _storeBound.ListAdd(DateTimeOffset.Now.ToUnixTimeMilliseconds() + (long)expiresIn.TotalMilliseconds,
            BitConverter.GetBytes(tempAuthKeyId), expiresIn, authKeyId);
        return true;
    }

    public long? GetBoundAuthKey(long tempAuthKeyId)
    {
        var authBytes = _storeTemp.Get(tempAuthKeyId);
        if (authBytes == null)
        {
            return null;
        }
        var authKeyId = BitConverter.ToInt64(authBytes);
        var tempBytes = _storeAuth.Get(authKeyId);
        if (tempBytes == null)
        {
            return null;
        }
        var boundKey = BitConverter.ToInt64(tempBytes);
        if (boundKey == tempAuthKeyId)
        {
            return authKeyId;
        }
        return null;
    }

    public async ValueTask<long?> GetBoundAuthKeyAsync(long tempAuthKeyId)
    {
        var authBytes = await _storeTemp.GetAsync(tempAuthKeyId);
        if (authBytes == null)
        {
            return null;
        }
        var authKeyId = BitConverter.ToInt64(authBytes);
        var tempBytes = await _storeAuth.GetAsync(authKeyId);
        if (tempBytes == null)
        {
            return null;
        }
        var boundKey = BitConverter.ToInt64(tempBytes);
        if (boundKey == tempAuthKeyId)
        {
            return authKeyId;
        }
        return null;
    }

    public IReadOnlyList<long> GetTempAuthKeys(long authKeyId)
    {
        _storeBound.ListDeleteByScore(DateTimeOffset.Now.ToUnixTimeMilliseconds(), authKeyId);
        var queryResult = _storeBound.ListGet(authKeyId);
        List<long> result = new List<long>();
        foreach (var v in queryResult)
        {
            result.Add(BitConverter.ToInt64(v));
        }
        return result;
    }
}