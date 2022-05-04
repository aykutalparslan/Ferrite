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
using System.Buffers;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL.layer139;
using MessagePack;

namespace Ferrite.TL;

public class UpdatesManager : IUpdatesManager
{
    private readonly ISessionService _sessions;
    private readonly ITLObjectFactory _factory;
    private readonly IDistributedPipe _pipe;
    public UpdatesManager(ISessionService sessions, ITLObjectFactory factory, IDistributedPipe pipe)
    {
        _sessions = sessions;
        _factory = factory;
        _pipe = pipe;
    }

    public async Task<bool> SendUpdateLoginToken(long authKeyId)
    {
        var update = _factory.Resolve<UpdateLoginTokenImpl>();
        var sessions = await _sessions.GetSessionsAsync(authKeyId);
        bool result = sessions.Count > 0;
        foreach (var sess in sessions)
        {
            MTProtoMessage message = new MTProtoMessage();
            message.SessionId = sess.SessionId;
            message.IsResponse = true;
            message.IsContentRelated = true;
            message.Data = update.TLBytes.ToArray();

            var bytes = MessagePackSerializer.Serialize(message);
            //TODO: maybe don't queue if the client is connected to the same server 
            _ = _pipe.WriteAsync(sess.NodeId.ToString(), bytes);
        }
        return result;
    }
}

