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

public class AuthKeyRepository : IAuthKeyRepository
{
    private readonly IKVStore _store;
    private readonly IVolatileKVStore _storeTemp;
    public AuthKeyRepository(IKVStore store, IVolatileKVStore storeTemp)
    {
        _store = store;
        _store.SetSchema(new TableDefinition("ferrite", "auth_keys",
            new KeyDefinition("pk",
                new DataColumn { Name = "auth_key_id", Type = DataType.Long })));
        _storeTemp = storeTemp;
        _storeTemp.SetSchema(new TableDefinition("ferrite", "auth_keys",
            new KeyDefinition("pk",
                new DataColumn { Name = "auth_key_id", Type = DataType.Long })));
    }
    public bool PutAuthKey(long authKeyId, byte[] authKey)
    {
        _storeTemp.Put(authKey, null, authKeyId);
        return _store.Put(authKey, authKeyId);
    }

    public byte[]? GetAuthKey(long authKeyId)
    {
        var val = _storeTemp.Get(authKeyId);
        if (val == null)
        {
            val = _store.Get(authKeyId);
            if (val != null)
            {
                _storeTemp.Put(val, null, authKeyId);
            }
        }
        return val;
    }

    public async ValueTask<byte[]?> GetAuthKeyAsync(long authKeyId)
    {
        var val = await _storeTemp.GetAsync(authKeyId);
        if (val == null)
        {
            val = await _store.GetAsync(authKeyId);
            if (val != null)
            {
                _storeTemp.Put(val, null, authKeyId);
            }
        }
        return val;
    }

    public bool DeleteAuthKey(long authKeyId)
    {
        _storeTemp.Delete(authKeyId);
        return  _store.Delete(authKeyId);
    }
}