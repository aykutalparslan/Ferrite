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
using Ferrite.Core.Connection;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using Ferrite.TL.slim;
using Ferrite.Utils;
using MessagePack;
using VectorOfLong = Ferrite.TL.VectorOfLong;

namespace Ferrite.Core.RequestChain;

public class MsgContainerProcessor : ILinkedHandler
{
    private readonly ILifetimeScope _scope;
    private readonly ISessionService _sessionManager;
    private readonly IMessagePipe _pipe;
    private readonly ILogger _log;
    public MsgContainerProcessor(ILifetimeScope scope, ISessionService sessionManager, IMessagePipe pipe, ILogger log)
    {
        _scope = scope;
        _sessionManager = sessionManager;
        _pipe = pipe;
        _log = log;
    }
    
    public ILinkedHandler SetNext(ILinkedHandler value)
    {
        Next = value;
        return Next;
    }

    public ILinkedHandler? Next { get; set; }

    public async ValueTask Process(object? sender, ITLObject input, TLExecutionContext ctx)
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
                if (Next != null) await Next.Process(sender, msg, ctx);
            }
            Services.MTProtoMessage message = new Services.MTProtoMessage();
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
            if (Next != null) await Next.Process(sender, input, ctx);
        }
    }

    public async ValueTask Process(object? sender, TLBytes input, TLExecutionContext ctx)
    {
        if (Next != null) await Next.Process(sender, input, ctx);
    }
}

