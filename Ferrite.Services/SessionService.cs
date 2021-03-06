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

using System;
using System.Collections.Concurrent;
using Ferrite.Data;
using MessagePack;

namespace Ferrite.Services;

public class SessionService : ISessionService
{
    public Guid NodeId { get; private set; }
    private readonly ConcurrentDictionary<long, MTProtoSession> _localSessions = new();
    private readonly ConcurrentDictionary<Nonce, MTProtoSession> _localAuthSessions = new();
    private readonly IDistributedCache _cache;
    private Guid GetNodeId()
    {
        if (File.Exists("node.guid"))
        {
            var bytes = File.ReadAllBytes("node.guid");
            return new Guid(bytes);
        }
        else
        {
            var guid = Guid.NewGuid();
            File.WriteAllBytes("node.guid", guid.ToByteArray());
            return guid;
        }
    }
    public SessionService(IDistributedCache cache)
    {
        NodeId = GetNodeId();
        _cache = cache;
    }
    public async Task<bool> AddSessionAsync(SessionState state, MTProtoSession session)
    {
        state.NodeId = NodeId;
        var remoteAdd = await _cache.PutSessionAsync(state.SessionId, MessagePackSerializer.Serialize(state),
            new TimeSpan(0,0, FerriteConfig.SessionTTL));
        var authKeyAdd = await _cache.PutSessionForAuthKeyAsync(state.AuthKeyId, state.SessionId);
        if (_localSessions.ContainsKey(state.SessionId))
        {
            _localSessions.Remove(state.SessionId, out var value);
        }
        return remoteAdd && authKeyAdd && _localSessions.TryAdd(state.SessionId, session);
    }

    public bool AddSession(SessionState state, MTProtoSession session)
    {
        state.NodeId = NodeId;
        var remoteAdd = _cache.PutSession(state.SessionId, MessagePackSerializer.Serialize(state),
            new TimeSpan(0,0, FerriteConfig.SessionTTL));
        var authKeyAdd = _cache.PutSessionForAuthKey(state.AuthKeyId, state.SessionId);
        if (_localSessions.ContainsKey(state.SessionId))
        {
            _localSessions.Remove(state.SessionId, out var value);
        }
        return remoteAdd && authKeyAdd && _localSessions.TryAdd(state.SessionId, session);
    }

    public async Task<SessionState?> GetSessionStateAsync(long sessionId)
    {
        return await GetSessionState(sessionId);
    }

    private async Task<SessionState> GetSessionState(long sessionId)
    {
        var rawSession = await _cache.GetSessionAsync(sessionId);
        if (rawSession != null)
        {
            var state = MessagePackSerializer.Deserialize<SessionState>(rawSession);
            if (state.ServerSalt.ValidSince <= (DateTimeOffset.Now.ToUnixTimeSeconds() - 1800))
            {
                state.ServerSaltOld = state.ServerSalt;
                var salt = new ServerSaltDTO();
                state.ServerSalt = salt;
                await _cache.PutSessionAsync(state.SessionId, MessagePackSerializer.Serialize(state),
                    new TimeSpan(0, 0, FerriteConfig.SessionTTL));
            }
            return state;
        }
        return null;
    }

    public async Task<bool> RemoveSession(long authKeyId, long sessionId)
    {
        await _cache.DeleteSessionAsync(sessionId);
        await _cache.DeleteSessionForAuthKeyAsync(authKeyId, sessionId);
        return _localSessions.TryRemove(sessionId, out var session);
    }
    public bool LocalSessionExists(long sessionId)
    {
        return _localSessions.ContainsKey(sessionId);
    }
    public bool TryGetLocalSession(long sessionId, out MTProtoSession session)
    {
        return _localSessions.TryGetValue(sessionId, out session);
    }

    public async Task<bool> AddAuthSessionAsync(byte[] nonce, AuthSessionState state, MTProtoSession session)
    {
        state.NodeId = NodeId;
        var remoteAdd = await _cache.PutAuthKeySessionAsync(nonce, MessagePackSerializer.Serialize(state));
        var key = (Nonce)nonce;
        if (_localAuthSessions.ContainsKey(key))
        {
            _localAuthSessions.Remove(key, out var value);
        }
        return remoteAdd && _localAuthSessions.TryAdd((Nonce)nonce, session);
    }

    public async Task<bool> UpdateAuthSessionAsync(byte[] nonce, AuthSessionState state)
    {
        return await _cache.PutAuthKeySessionAsync(nonce, MessagePackSerializer.Serialize(state));
    }

    public async Task<AuthSessionState?> GetAuthSessionStateAsync(byte[] nonce)
    {
        var rawSession = await _cache.GetAuthKeySessionAsync(nonce);
        if (rawSession != null)
        {
            var state = MessagePackSerializer.Deserialize<AuthSessionState>(rawSession);
            
            return state;
        }
        return null;
    }

    public bool LocalAuthSessionExists(byte[] nonce)
    {
        return _localAuthSessions.ContainsKey((Nonce)nonce);
    }

    public bool TryGetLocalAuthSession(byte[] nonce, out MTProtoSession session)
    {
        return _localAuthSessions.TryGetValue((Nonce)nonce, out session);
    }

    public bool RemoveAuthSession(byte[] nonce)
    {
        _cache.RemoveAuthKeySessionAsync(nonce);
        return _localAuthSessions.TryRemove((Nonce)nonce, out var a);
    }

    public async Task<bool> OnPing(long authKeyId, long sessionId)
    {
        var ttlSet = await _cache.SetSessionTTLAsync(sessionId, new TimeSpan(0, 0, FerriteConfig.SessionTTL));
        return ttlSet && await _cache.PutSessionForAuthKeyAsync(authKeyId, sessionId);
    }

    public async Task<ICollection<SessionState>> GetSessionsAsync(long authKeyId)
    {
        var sessionIds = await _cache.GetSessionsByAuthKeyAsync(authKeyId,
            new TimeSpan(0, 0, FerriteConfig.SessionTTL));

        List<SessionState> result = new();
        foreach (var sessionId in sessionIds)
        {
            var state = await GetSessionState(sessionId);
            if (state != null)
            {
                result.Add(state);
            }
        }
        return result;
    }
}

