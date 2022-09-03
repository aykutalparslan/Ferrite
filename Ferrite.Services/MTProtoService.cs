﻿//
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
using Ferrite.Data.Repositories;
using Ferrite.Utils;

namespace Ferrite.Services;

public class MTProtoService : IMTProtoService
{
    private readonly IPersistentStore _store;
    private readonly IDistributedCache _cache;
    private readonly IMTProtoTime _time;
    private readonly IUnitOfWork _unitOfWork;
    public MTProtoService(IPersistentStore store, IDistributedCache cache, IMTProtoTime time,
        IUnitOfWork unitOfWork)
    {
        _store = store;
        _cache = cache;
        _time = time;
        _unitOfWork = unitOfWork;
    }

    public async Task<ICollection<ServerSaltDTO>> GetServerSaltsAsync(long authKeyId, int count)
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

    public async Task<bool> PutAuthKeyAsync(long authKeyId, byte[] authKey)
    {
        var result = _unitOfWork.AuthKeyRepository.PutAuthKey(authKeyId, authKey);
        return result && await _unitOfWork.SaveAsync();
    }

    public bool PutAuthKey(long authKeyId, byte[] authKey)
    {
        var result = _unitOfWork.AuthKeyRepository.PutAuthKey(authKeyId, authKey);
        return result && _unitOfWork.Save();
    }

    public byte[]? GetAuthKey(long authKeyId)
    {
        return _unitOfWork.AuthKeyRepository.GetAuthKey(authKeyId);
    }

    public async Task<byte[]?> GetAuthKeyAsync(long authKeyId)
    {
        return await _unitOfWork.AuthKeyRepository.GetAuthKeyAsync(authKeyId);
    }

    public bool PutTempAuthKey(long authKeyId, byte[] authKey, TimeSpan expiresIn)
    {
        var result = _unitOfWork.TempAuthKeyRepository.PutTempAuthKey(authKeyId, authKey, expiresIn);
        return result && _unitOfWork.Save();
    }

    public async Task<bool> PutTempAuthKeyAsync(long authKeyId, byte[] authKey, TimeSpan expiresIn)
    {
        var result = _unitOfWork.TempAuthKeyRepository.PutTempAuthKey(authKeyId, authKey, expiresIn);
        return result && await _unitOfWork.SaveAsync();
    }

    public byte[]? GetTempAuthKey(long authKeyId)
    {
        return _unitOfWork.TempAuthKeyRepository.GetTempAuthKey(authKeyId);
    }

    public async Task<byte[]?> GetTempAuthKeyAsync(long authKeyId)
    {
        return await _unitOfWork.TempAuthKeyRepository.GetTempAuthKeyAsync(authKeyId);
    }

    public async Task<bool> PutBoundAuthKey(long tempAuthKeyId, long authKeyId, TimeSpan expiresIn)
    {
        var result = _unitOfWork.BoundAuthKeyRepository.PutBoundAuthKey(tempAuthKeyId, authKeyId, expiresIn);
        return result && await _unitOfWork.SaveAsync();
    }

    public async ValueTask<long?> GetBoundAuthKeyAsync(long tempAuthKeyId)
    {
        return await _unitOfWork.BoundAuthKeyRepository.GetBoundAuthKeyAsync(tempAuthKeyId);
    }

    public async Task<bool> DestroyAuthKeyAsync(long authKeyId)
    {
        var success = _unitOfWork.AuthKeyRepository.DeleteAuthKey(authKeyId);
        return success && await _unitOfWork.SaveAsync();
    }
}

