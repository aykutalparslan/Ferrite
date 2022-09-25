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

using System.Security.Cryptography;
using Ferrite.Utils;
using MessagePack;

namespace Ferrite.Data.Repositories;

/* Server Salt
 * 
 * A (random) 64-bit number changed every 30 minutes (separately for each session) at the request of the server. 
 * All subsequent messages must contain the new salt (although, messages with the old salt are still accepted for
 * a further 1800 seconds). Required to protect against replay attacks and certain tricks associated with adjusting
 * the client clock to a moment in the distant future.
 *
 * The client may at any time request from the server several (between 1 and 64) future server salts together with
 * their validity periods. Having stored them in persistent memory, the client may use them to send messages
 * in the future even if it changes sessions (a server salt is attached to the authorization key rather than
 * being session-specific).
 */

public class ServerSaltRepository : IServerSaltRepository
{
    private readonly IVolatileKVStore _store;
    private readonly IVolatileKVStore _validityStore;
    public ServerSaltRepository(IVolatileKVStore store, IVolatileKVStore validityStore)
    {
        _store = store;
        store.SetSchema(new TableDefinition("ferrite", "server_salts",
            new KeyDefinition("pk",
                new DataColumn { Name = "auth_key_id", Type = DataType.Long })));
        _validityStore = validityStore;
        _validityStore.SetSchema(new TableDefinition("ferrite", "server_salt_validity",
            new KeyDefinition("pk",
                new DataColumn { Name = "auth_key_id", Type = DataType.Long },
                new DataColumn { Name = "server_salt", Type = DataType.Long })));
    }
    public bool PutServerSalt(long authKeyId, ServerSaltDTO salt, int TTL)
    {
        var saltBytes = MessagePackSerializer.Serialize(salt);
        var expire = TimeSpan.FromSeconds(TTL);
        _store.ListAdd(DateTimeOffset.Now.AddSeconds(TTL).ToUnixTimeMilliseconds(),
            saltBytes);
        _validityStore.Put(saltBytes, expire, authKeyId, salt.Salt);
        return true;
    }

    public IReadOnlyCollection<ServerSaltDTO> GetServerSalts(long authKeyId, int count)
    {
        count = Math.Min(count, 64);
        List<ServerSaltDTO> salts = new();
        var existing = _store.ListGet(authKeyId);
        foreach (var b in existing)
        {
            var salt = MessagePackSerializer.Deserialize<ServerSaltDTO>(b);
            if (salt.ValidSince + 3600 >= DateTimeOffset.Now.AddHours(1).ToUnixTimeSeconds())
            {
                salts.Add(salt);
            }
            else
            {
                _store.ListDelete(b, authKeyId);
            }
        }

        if (salts.Count > count)
        {
            salts = (from s in salts orderby s.ValidSince select s).Take(count).ToList();
        }
        else if (salts.Count == 0)
        {
            Span<byte> randomBytes = stackalloc byte[8];
            int validSince = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
            for (int i = 0; i < count; i++)
            {
                RandomNumberGenerator.Fill(randomBytes);
                long salt = BitConverter.ToInt64(randomBytes);
                ServerSaltDTO s = new ServerSaltDTO(salt, validSince);
                validSince += 1800;
                salts.Add(s);
                var saltBytes = MessagePackSerializer.Serialize(s);
                _store.ListAdd((long)(validSince + 1800) * 1000, saltBytes);
                int ttl = validSince - (int)DateTimeOffset.Now.ToUnixTimeSeconds() + 1800;
                _validityStore.Put(saltBytes,
                    TimeSpan.FromSeconds(ttl),
                    authKeyId, salt);
            }
        }
        return salts;
    }

    public ValueTask<IReadOnlyCollection<ServerSaltDTO>> GetServerSaltsAsync(long authKeyId, int count)
    {
        return new ValueTask<IReadOnlyCollection<ServerSaltDTO>>(GetServerSalts(authKeyId, count));
    }

    public long GetServerSaltValidity(long authKeyId, long serverSalt)
    {
        return BitConverter.ToInt64(_validityStore.Get(authKeyId,serverSalt));
    }

    public ValueTask<long> GetServerSaltValidityAsync(long authKeyId, long serverSalt)
    {
        return new ValueTask<long>(GetServerSaltValidity(authKeyId, serverSalt));
    }
}