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
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using Ferrite.Utils;
using MessagePack;

namespace Ferrite.Core;

public class MsgContainerProcessor : IProcessor
{
    private readonly ILifetimeScope _scope;
    private readonly ISessionService _sessionManager;
    private readonly IDistributedPipe _pipe;
    private readonly ILogger _log;
    public MsgContainerProcessor(ILifetimeScope scope, ISessionService sessionManager, IDistributedPipe pipe, ILogger log)
    {
        _scope = scope;
        _sessionManager = sessionManager;
        _pipe = pipe;
        _log = log;
    }

    public async Task Process(object? sender, ITLObject input, Queue<ITLObject> output, TLExecutionContext ctx)
    {
        if (input.Constructor == TLConstructor.MsgContainer &&
            input is MsgContainer container)
        {
            _log.Information(String.Format($"MsgContainer received with Id: {ctx.MessageId}"));
            var ack = _scope.Resolve<MsgsAck>();
            ack.MsgIds = new VectorOfLong(container.Messages.Count+1);
            ack.MsgIds.Add(ctx.MessageId);
            foreach (var msg in container.Messages)
            {
                ack.MsgIds.Add(msg.MsgId);
                output.Enqueue(msg);
            }
            MTProtoMessage message = new MTProtoMessage();
            message.SessionId = ctx.SessionId;
            message.IsResponse = true;
            message.IsContentRelated = true;
            message.Data = ack.TLBytes.ToArray();
            if(sender is MTProtoConnection connection)
            {
                await connection.SendAsync(message);
            }
            else if (await _sessionManager.GetSessionStateAsync(ctx.SessionId)
                    is { } session)
            {
                var bytes = MessagePackSerializer.Serialize(message);
                _ = _pipe.WriteMessageAsync(session.NodeId.ToString(), bytes);
            }
        }
        else
        {
            output.Enqueue(input);
        }
    }

    public async Task Process(object? sender, IMemoryOwner<byte> input, Queue<IMemoryOwner<byte>> output, TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }
}

