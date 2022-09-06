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

using System.Text;

namespace Ferrite.Data.Repositories;

public class PhoneCodeRepository : IPhoneCodeRepository
{
    private readonly IVolatileKVStore _store;
    public PhoneCodeRepository(IVolatileKVStore store)
    {
        _store = store;
        store.SetSchema(new TableDefinition("ferrite", "phone_codes",
            new KeyDefinition("pk",
                new DataColumn { Name = "phone_number", Type = DataType.String },
                new DataColumn { Name = "phone_code_hash", Type = DataType.String })));
    }
    public void PutPhoneCodeAsync(string phoneNumber, string phoneCodeHash, string phoneCode, TimeSpan expiresIn)
    {
        _store.Put(Encoding.UTF8.GetBytes(phoneCode), expiresIn, phoneNumber, phoneCodeHash);
    }

    public string? GetPhoneCode(string phoneNumber, string phoneCodeHash)
    {
        var bytes = _store.Get(phoneNumber, phoneCodeHash);
        if (bytes is { Length: > 0 })
        {
            return Encoding.UTF8.GetString(bytes);
        }

        return null;
    }

    public bool DeletePhoneCode(string phoneNumber, string phoneCodeHash)
    {
        _store.Delete(phoneNumber, phoneCodeHash);
        return true;
    }
}