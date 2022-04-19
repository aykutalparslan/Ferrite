/*
 *   Project Ferrite is an Implementation of the Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
using System;
using Ferrite.Data.Auth;

namespace Ferrite.Data;

public interface IDistributedStore
{
    public IAtomicCounter GetCounter(string name);
    public Task<byte[]> GetAuthKeyAsync(long authKeyId);
    public Task<bool> PutAuthKeyAsync(long authKeyId, byte[] authKey);
    public Task<byte[]> GetSessionAsync(long sessionId);
    public Task<bool> PutSessionAsync(long sessionId, byte[] sessionData);
    public Task<bool> RemoveSessionAsync(long sessionId);
    public Task<byte[]> GetAuthKeySessionAsync(byte[] nonce);
    public Task<bool> PutAuthKeySessionAsync(byte[] nonce, byte[] sessionData);
    public Task<bool> RemoveAuthKeySessionAsync(byte[] nonce);
    public Task<string> GetPhoneCodeAsync(string phoneNumber, string phoneCodeHash);
    public Task<bool> DeletePhoneCodeAsync(string phoneNumber, string phoneCodeHash);
    public Task<bool> PutPhoneCodeAsync(string phoneNumber, string phoneCodeHash, string phoneCode, TimeSpan expiresIn);
    public Task<bool> PutServerSaltAsync(long authKeyId, long serverSalt, long validSince, TimeSpan expiresIn);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="authKeyId"></param>
    /// <param name="serverSalt"></param>
    /// <returns>ValidSince</returns>
    public Task<long> GetServerSaltValidityAsync(long authKeyId, long serverSalt);
}

