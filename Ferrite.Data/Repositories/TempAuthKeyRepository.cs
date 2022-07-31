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

public class TempAuthKeyRepository : ITempAuthKeyRepository
{
    private readonly IKVStore _store;
    public TempAuthKeyRepository(IKVStore store)
    {
        _store = store;
        store.SetSchema(new TableDefinition("ferrite", "temp_auth_keys",
            new KeyDefinition("pk",
                new DataColumn { Name = "auth_key_id", Type = DataType.Long })));
    }
    public bool PutTempAuthKey(long tempAuthKeyId, byte[] tempAuthKey, TimeSpan expiresIn)
    {
        return _store.Put(tempAuthKey, tempAuthKeyId);
    }

    public byte[]? GetTempAuthKey(long tempAuthKeyId)
    {
        return _store.Get(tempAuthKeyId);
    }

    public async ValueTask<byte[]?> GetTempAuthKeyAsync(long tempAuthKeyId)
    {
        return await _store.GetAsync(tempAuthKeyId);
    }
}