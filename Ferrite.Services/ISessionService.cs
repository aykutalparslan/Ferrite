/*
 *   Project Ferrite is an Implementation Telegram Server API
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


namespace Ferrite.Services;

public interface ISessionService
{
    Guid NodeId { get; }

    Task<bool> AddSessionAsync(long authKeyId, long sessionId, MTProtoSession session);
    bool AddSession(long authKeyId, long sessionId, MTProtoSession session);
    Task<SessionState?> GetSessionStateAsync(long sessionId);
    Task<bool> DeleteSessionAsync(long sessionId);
    Task<ICollection<SessionState>> GetSessionsAsync(long authKeyId);
    Task<bool> AddAuthSessionAsync(byte[] nonce, AuthSessionState state, MTProtoSession session);
    public Task<bool> UpdateAuthSessionAsync(byte[] nonce, AuthSessionState state);
    Task<AuthSessionState?> GetAuthSessionStateAsync(byte[] nonce);
    bool LocalSessionExists(long sessionId);
    bool LocalAuthSessionExists(byte[] nonce);
    Task<bool> RemoveSession(long authKeyId, long sessionId);
    Task<bool> OnPing(long authKeyId, long sessionId);
    bool RemoveAuthSession(byte[] nonce);
    bool TryGetLocalSession(long sessionId, out MTProtoSession session);
    bool TryGetLocalAuthSession(byte[] nonce, out MTProtoSession session);
}
