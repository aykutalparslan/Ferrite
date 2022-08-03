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
    private readonly IKVStore _storeTemp;
    private readonly IKVStore _storeAuth;
    public BoundAuthKeyRepository(IKVStore storeTemp, IKVStore storeAuth)
    {
        _storeTemp = storeTemp;
        _storeAuth = storeAuth;
        _storeTemp.SetSchema(new TableDefinition("ferrite", "bound_auth_keys_temp",
            new KeyDefinition("pk",
                new DataColumn { Name = "temp_auth_key_id", Type = DataType.Long })));
        _storeAuth.SetSchema(new TableDefinition("ferrite", "bound_auth_keys_auth",
            new KeyDefinition("pk",
                new DataColumn { Name = "auth_key_id", Type = DataType.Long })));
    }
    
    public bool PutBoundAuthKey(long tempAuthKeyId, long authKeyId, TimeSpan expiresIn)
    {
        throw new NotImplementedException();
    }

    public long? GetBoundAuthKey(long tempAuthKeyId)
    {
        throw new NotImplementedException();
    }

    public async ValueTask<long?> GetBoundAuthKeyAsync(long tempAuthKeyId)
    {
        throw new NotImplementedException();
    }

    public IReadOnlyCollection<long> GetTempAuthKeys(long authKeyId)
    {
        throw new NotImplementedException();
    }
}