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

public class SessionRepository : ISessionRepository
{
    private readonly IVolatileKVStore _store;
    public SessionRepository(IVolatileKVStore store)
    {
        _store = store;
        store.SetSchema(new TableDefinition("ferrite", "sessions",
            new KeyDefinition("pk",
                new DataColumn { Name = "session_id", Type = DataType.Long })));
    }
    public bool PutSession(long sessionId, byte[] sessionData, TimeSpan expire)
    {
        _store.Put(sessionData, expire, sessionId);
        return true;
    }

    public byte[] GetSession(long sessionId)
    {
        return _store.Get(sessionId);
    }

    public bool SetSessionTTL(long sessionId, TimeSpan expire)
    {
        _store.UpdateTtl(expire, sessionId);
        return true;
    }

    public bool DeleteSession(long sessionId)
    {
        _store.Delete(sessionId);
        return true;
    }

    public bool PutSessionForAuthKey(long authKeyId, long sessionId)
    {
        return _store.ListAdd(DateTimeOffset.Now.ToUnixTimeMilliseconds(), 
            BitConverter.GetBytes(sessionId), null, authKeyId);
    }

    public bool DeleteSessionForAuthKey(long authKeyId, long sessionId)
    {
        return _store.ListDelete(BitConverter.GetBytes(sessionId), null, authKeyId);
    }

    public ICollection<long> GetSessionsByAuthKey(long authKeyId, TimeSpan expire)
    {
        var time = DateTimeOffset.Now - expire;
        _store.ListDeleteByScore(time.ToUnixTimeMilliseconds());
        return _store.ListGet(authKeyId).Select(_ => BitConverter.ToInt64(_)).ToList();
    }
}