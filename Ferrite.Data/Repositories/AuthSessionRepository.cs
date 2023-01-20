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

public class AuthSessionRepository : IAuthSessionRepository
{
    private readonly IVolatileKVStore _store;
    public AuthSessionRepository(IVolatileKVStore store)
    {
        _store = store;
        store.SetSchema(new TableDefinition("ferrite", "auth_sessions",
            new KeyDefinition("pk",
                new DataColumn { Name = "nonce", Type = DataType.Bytes })));
    }
    public bool PutAuthKeySession(byte[] nonce, byte[] sessionData)
    {
        _store.Put(sessionData, null, nonce);
        return true;
    }

    public byte[]? GetAuthKeySession(byte[] nonce)
    {
        return _store.Get(nonce);
    }

    public bool RemoveAuthKeySession(byte[] nonce)
    {
        _store.Delete(nonce);
        return true;
    }
}