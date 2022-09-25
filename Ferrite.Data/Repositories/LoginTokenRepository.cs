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

using Ferrite.Data.Auth;
using MessagePack;

namespace Ferrite.Data.Repositories;

public class LoginTokenRepository : ILoginTokenRepository
{
    private readonly IVolatileKVStore _store;
    public LoginTokenRepository(IVolatileKVStore store)
    {
        _store = store;
        store.SetSchema(new TableDefinition("ferrite", "login_tokens",
            new KeyDefinition("pk",
                new DataColumn { Name = "token", Type = DataType.Bytes })));
    }
    public bool PutLoginToken(LoginViaQRDTO login, TimeSpan expiresIn)
    {
        var tokenBytes = MessagePackSerializer.Serialize(login);
        _store.Put(tokenBytes, expiresIn, login.Token);
        return true;
    }

    public LoginViaQRDTO? GetLoginToken(byte[] token)
    {
        var tokenBytes = _store.Get(token);
        if (tokenBytes == null)
        {
            return null;
        }

        return MessagePackSerializer.Deserialize<LoginViaQRDTO>(tokenBytes);
    }
}