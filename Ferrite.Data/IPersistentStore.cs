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
namespace Ferrite.Data;
public interface IPersistentStore
{
    public Task SaveAuthKeyAsync(long authKeyId, byte[] authKey);
    public Task<byte[]?> GetAuthKeyAsync(long authKeyId);
    public Task SaveAuthKeyDetailsAsync(AuthKeyDetails details);
    public Task<AuthKeyDetails?> GetAuthKeyDetailsAsync(long authKeyId);
    public Task<bool> DeleteAuthKeyAsync(long authKeyId);
    public Task SaveServerSaltAsync(long authKeyId, long serverSalt, long validSince, int TTL);
    public Task<ICollection<ServerSalt>> GetServerSaltsAsync(long authKeyId, int count);
    public Task<bool> SaveUserAsync(User user);
    public Task<bool> UpdateUserAsync(User user);
    public Task<User?> GetUserAsync(long userId);
    public Task<User?> GetUserAsync(string phone);
    public Task<User?> GetUserByUsernameAsync(string username);
}

