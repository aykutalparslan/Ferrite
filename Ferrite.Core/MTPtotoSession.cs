/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
using System.Diagnostics.CodeAnalysis;

namespace Ferrite.Core
{
    public class MTPtotoSession
    {
        private readonly long _id;
        private readonly WeakReference<MTProtoConnection> _ref;
        public MTPtotoSession(long sessionId, MTProtoConnection connection)
        {
            _id = sessionId;
            _ref = new(connection);
        }

        public long SessionId => _id;

        public bool TryGetConnection([NotNullWhen(true)] out MTProtoConnection? connection)
        {
            return _ref.TryGetTarget(out connection);
        }
    }
}

