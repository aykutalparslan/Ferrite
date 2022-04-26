//
//  Project Ferrite is an Implementation of the Telegram Server API
//  Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using System.Security.Cryptography;
using Ferrite.Data;
using Ferrite.Utils;

namespace Ferrite.Services;

public class MTProtoService : IMTProtoService
{
    private readonly IPersistentStore _store;
    private readonly IDistributedStore _cache;
    private readonly IMTProtoTime _time;
    public MTProtoService(IPersistentStore store, IDistributedStore cache, IMTProtoTime time)
    {
        _store = store;
        _cache = cache;
        _time = time;
    }

    public async Task<ICollection<ServerSalt>> GetServerSaltsAsync(long authKeyId, int count)
    {
        var serverSalts = await _store.GetServerSaltsAsync(authKeyId, count);
        if (serverSalts.Count == 0)
        {
            await GenerateSalts(authKeyId);
        }
        return await _store.GetServerSaltsAsync(authKeyId, count);
    }

    private async Task GenerateSalts(long authKeyId)
    {
        var time = _time.GetUnixTimeInSeconds();
        int offset = 0;
        byte[] saltBytes = new byte[8];
        for (int i = 0; i < 64; i++)
        {
            RandomNumberGenerator.Fill(saltBytes);
            long salt = BitConverter.ToInt64(saltBytes);
            await _store.SaveServerSaltAsync(authKeyId, salt, time + offset, offset + 3600);
            await _cache.PutServerSaltAsync(authKeyId, salt, time + offset, new TimeSpan(0, 0, offset + 3600));
            offset += 3600;
        }
    }

    public async Task<long> GetServerSaltValidityAsync(long authKeyId, long serverSalt)
    {
        long validSince = await _cache.GetServerSaltValidityAsync(authKeyId, serverSalt);
        if(validSince == 0)
        {
            var serverSalts = _store.GetServerSaltsAsync(authKeyId, 64);
            if (serverSalts.Result.Count == 0)
            {
                _ = GenerateSalts(authKeyId);
            }
        }
        return validSince;
    }

    public async Task<byte[]?> GetAuthKeyAsync(long authKeyId)
    {
        var authKey = await _cache.GetAuthKeyAsync(authKeyId);
        if (authKey == null)
        {
            authKey = await _store.GetAuthKeyAsync(authKeyId);
        }
        return authKey;
    }
}

