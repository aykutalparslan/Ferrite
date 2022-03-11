/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
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

