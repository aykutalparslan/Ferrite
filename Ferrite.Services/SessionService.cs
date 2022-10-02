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
using Ferrite.Data.Repositories;
using Ferrite.Utils;
using MessagePack;

namespace Ferrite.Services;

public class SessionService : ISessionService
{
    public Guid NodeId { get; private set; }
    private readonly ConcurrentDictionary<long, MTProtoSession> _localSessions = new();
    private readonly ConcurrentDictionary<Nonce, MTProtoSession> _localAuthSessions = new();
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger _log;
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
    public SessionService(IUnitOfWork unitOfWork, ILogger log)
    {
        NodeId = GetNodeId();
        _unitOfWork = unitOfWork;
        _log = log;
    }
    public async Task<bool> AddSessionAsync(long authKeyId, long sessionId, MTProtoSession session)
    {
        var state = new SessionState
        {
            SessionId = sessionId,
            NodeId = NodeId,
        };
        var remoteAdd =  _unitOfWork.SessionRepository.PutSession(state.SessionId, MessagePackSerializer.Serialize(state),
            new TimeSpan(0,0, FerriteConfig.SessionTTL));
        var authKeyAdd = _unitOfWork.SessionRepository.PutSessionForAuthKey(authKeyId, state.SessionId);
        _log.Debug($"=== 1 = Put Session for Auth Key: {authKeyId} ===");
        await _unitOfWork.SaveAsync();
        if (_localSessions.ContainsKey(state.SessionId))
        {
            _localSessions.Remove(state.SessionId, out var value);
        }
        return remoteAdd && authKeyAdd && _localSessions.TryAdd(state.SessionId, session);
    }

    public bool AddSession(long authKeyId, long sessionId, MTProtoSession session)
    {
        var state = new SessionState
        {
            SessionId = sessionId,
            NodeId = NodeId,
        };
        var remoteAdd = _unitOfWork.SessionRepository.PutSession(state.SessionId, MessagePackSerializer.Serialize(state),
            new TimeSpan(0,0, FerriteConfig.SessionTTL));
        var authKeyAdd = _unitOfWork.SessionRepository.PutSessionForAuthKey(authKeyId, state.SessionId);
        _log.Debug($"=== 2 = Put Session for Auth Key: {authKeyId} ===");
        _unitOfWork.Save();
        if (_localSessions.ContainsKey(state.SessionId))
        {
            _localSessions.Remove(state.SessionId, out var value);
        }

        var localAdd = _localSessions.TryAdd(state.SessionId, session);
        return remoteAdd && authKeyAdd && localAdd;
    }

    public async Task<SessionState?> GetSessionStateAsync(long sessionId)
    {
        return await GetSessionState(sessionId);
    }

    public async Task<bool> DeleteSessionAsync(long sessionId)
    {
        _localSessions.TryRemove(sessionId, out var removed);
        _unitOfWork.SessionRepository.DeleteSession(sessionId);
        await _unitOfWork.SaveAsync();
        return true;
    }

    private async Task<SessionState> GetSessionState(long sessionId)
    {
        var rawSession = _unitOfWork.SessionRepository.GetSession(sessionId);
        if (rawSession != null)
        {
            var state = MessagePackSerializer.Deserialize<SessionState>(rawSession);
            return state;
        }
        return null;
    }

    public async Task<bool> RemoveSession(long authKeyId, long sessionId)
    {
        _unitOfWork.SessionRepository.DeleteSession(sessionId);
        _unitOfWork.SessionRepository.DeleteSessionForAuthKey(authKeyId, sessionId);
        await _unitOfWork.SaveAsync();
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
        var remoteAdd = _unitOfWork.AuthSessionRepository.PutAuthKeySession(nonce, MessagePackSerializer.Serialize(state));
        await _unitOfWork.SaveAsync();
        var key = (Nonce)nonce;
        if (_localAuthSessions.ContainsKey(key))
        {
            _localAuthSessions.Remove(key, out var value);
        }
        return remoteAdd && _localAuthSessions.TryAdd((Nonce)nonce, session);
    }

    public async Task<bool> UpdateAuthSessionAsync(byte[] nonce, AuthSessionState state)
    {
        bool result = _unitOfWork.AuthSessionRepository.PutAuthKeySession(nonce, MessagePackSerializer.Serialize(state));
        await _unitOfWork.SaveAsync();
        return result;
    }

    public async Task<AuthSessionState?> GetAuthSessionStateAsync(byte[] nonce)
    {
        var rawSession = _unitOfWork.AuthSessionRepository.GetAuthKeySession(nonce);
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
        _unitOfWork.AuthSessionRepository.RemoveAuthKeySession(nonce);
        _unitOfWork.Save();
        return _localAuthSessions.TryRemove((Nonce)nonce, out var a);
    }

    public async Task<bool> OnPing(long authKeyId, long sessionId)
    {
        var ttlSet = _unitOfWork.SessionRepository.SetSessionTTL(sessionId, new TimeSpan(0, 0, FerriteConfig.SessionTTL));
        bool sessionSaved = _unitOfWork.SessionRepository.PutSessionForAuthKey(authKeyId, sessionId);
        _log.Debug($"=== Put Session for Auth Key: {authKeyId} ===");
        await _unitOfWork.SaveAsync();
        return ttlSet && sessionSaved;
    }

    public async Task<ICollection<SessionState>> GetSessionsAsync(long authKeyId)
    {
        var sessionIds = _unitOfWork.SessionRepository.GetSessionsByAuthKey(authKeyId,
            TimeSpan.FromSeconds(FerriteConfig.SessionTTL));
        _log.Debug($"=== Got {sessionIds.Count} sessions for Auth Key: {authKeyId} ===");
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

