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

namespace Ferrite.Core;

public class SessionManager
{
    private readonly ConcurrentDictionary<long, MTPtotoSession> _sessions = new();
    public bool AddSession(long sessionId, MTPtotoSession session)
    {
        return _sessions.TryAdd(sessionId, session);
    }
    public bool RemoveSession(long sessionId)
    {
        return _sessions.TryRemove(sessionId, out var session);
    }
    public bool SessionExists(long sessionId)
    {
        return _sessions.ContainsKey(sessionId);
    }
    public bool TryGetSession(long sessionId, out MTPtotoSession session)
    {
        return _sessions.TryGetValue(sessionId, out session);
    }
}

