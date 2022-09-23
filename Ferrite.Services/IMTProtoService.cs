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
using Ferrite.Data;
using Nest;

namespace Ferrite.Services;

public interface IMTProtoService
{
    public Task<IReadOnlyCollection<ServerSaltDTO>> GetServerSaltsAsync(long authKeyId, int count);
    public Task<long> GetServerSaltValidityAsync(long authKeyId, long serverSalt);
    public Task<bool> PutAuthKeyAsync(long authKeyId, byte[] authKey);
    public bool PutAuthKey(long authKeyId, byte[] authKey);
    public byte[]? GetAuthKey(long authKeyId);
    public Task<byte[]?> GetAuthKeyAsync(long authKeyId);
    public bool PutTempAuthKey(long authKeyId, byte[] authKey, TimeSpan expiresIn);
    public Task<bool> PutTempAuthKeyAsync(long authKeyId, byte[] authKey, TimeSpan expiresIn);
    public byte[]? GetTempAuthKey(long authKeyId);
    public Task<byte[]?> GetTempAuthKeyAsync(long authKeyId);
    public Task<bool> PutBoundAuthKey(long tempAuthKeyId, long authKeyId, TimeSpan expiresIn);
    public ValueTask<long?> GetBoundAuthKeyAsync(long tempAuthKeyId);
    public Task<bool> DestroyAuthKeyAsync(long authKeyId);
}

