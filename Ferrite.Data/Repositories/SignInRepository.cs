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

public class SignInRepository : ISignInRepository
{
    private readonly IVolatileKVStore _store;
    public SignInRepository(IVolatileKVStore store)
    {
        _store = store;
        store.SetSchema(new TableDefinition("ferrite", "sign_ins",
            new KeyDefinition("pk",
                new DataColumn { Name = "phone_number", Type = DataType.String },
                new DataColumn { Name = "phone_code_hash", Type = DataType.String })));
    }
    public bool PutSignIn(long authKeyId, string phoneNumber, string phoneCodeHash)
    {
        _store.Put(BitConverter.GetBytes(authKeyId), null, phoneNumber, phoneCodeHash);
        return true;
    }

    public long GetSignIn(string phoneNumber, string phoneCodeHash)
    {
        var result = _store.Get(phoneNumber, phoneCodeHash);
        return result != null ? BitConverter.ToInt64(result) : 0;
    }
}