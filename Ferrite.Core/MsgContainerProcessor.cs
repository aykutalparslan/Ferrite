//
//  Project Ferrite is an Implementation Telegram Server API
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
using Autofac;
using Ferrite.Data;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using MessagePack;

namespace Ferrite.Core;

public class MsgContainerProcessor : IProcessor
{
    private readonly ILifetimeScope _scope;
    private readonly ISessionManager _sessionManager;
    private readonly IDistributedPipe _pipe;
    public MsgContainerProcessor(ILifetimeScope scope, ISessionManager sessionManager, IDistributedPipe pipe)
    {
        _scope = scope;
        _sessionManager = sessionManager;
        _pipe = pipe;
    }

    public async Task Process(object? sender, ITLObject input, Queue<ITLObject> output, TLExecutionContext ctx)
    {
        if (input.Constructor == TLConstructor.MsgContainer &&
            input is MsgContainer container)
        {
            var ack = _scope.Resolve<MsgsAck>();
            ack.MsgIds = new VectorOfLong(1);
            ack.MsgIds.Add(ctx.MessageId);
            foreach (var msg in container.Messages)
            {
                //ack.MsgIds.Add(msg.MsgId);
                output.Enqueue(msg);
            }
            
            MTProtoMessage message = new MTProtoMessage();
            message.SessionId = ctx.SessionId;
            message.IsResponse = true;
            message.IsContentRelated = true;
            message.Data = ack.TLBytes.ToArray();
            if (await _sessionManager.GetSessionStateAsync(ctx.SessionId)
                    is SessionState session)
            {
                var bytes = MessagePackSerializer.Serialize(message);
                _ = _pipe.WriteAsync(session.NodeId.ToString(), bytes);
            }
        }
        else
        {
            output.Enqueue(input);
        }
    }
}

