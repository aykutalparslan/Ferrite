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
using Ferrite.Data;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using MessagePack;

namespace Ferrite.Core;

public class MTProtoRequestProcessor : IProcessor
{
    private readonly ISessionManager _sessionManager;
    private readonly IDistributedPipe _pipe;
    public MTProtoRequestProcessor(ISessionManager sessionManager, IDistributedPipe pipe)
    {
        _sessionManager = sessionManager;
        _pipe = pipe;
    }
    public async Task Process(object? sender, ITLObject input, Queue<ITLObject> output, TLExecutionContext ctx)
    {
        Console.WriteLine(input.ToString());
        if (input is ITLMethod method)
        {
            var result = await method.ExecuteAsync(ctx);
            if (result != null)
            {
                MTProtoMessage message = new MTProtoMessage();
                message.SessionId = ctx.SessionId;
                message.IsResponse = true;
                message.IsContentRelated = true;
                message.Data = result.TLBytes.ToArray();

                if (await _sessionManager.GetSessionStateAsync(ctx.SessionId)
                    is SessionState session)
                {
                    var bytes = MessagePackSerializer.Serialize(message);
                    _ = _pipe.WriteAsync(session.NodeId.ToString(), bytes);
                }

                Console.WriteLine("-->" + result.ToString());
            }
        }
        if (input is Message msg && msg.Body is ITLMethod encMethod)
        {
            Console.WriteLine(encMethod.ToString());
            var _context = new TLExecutionContext(ctx.SessionData);
            _context.MessageId = msg.MsgId;
            _context.AuthKeyId = ctx.AuthKeyId;
            _context.SequenceNo = msg.Seqno;
            _context.Salt = ctx.Salt;
            _context.SessionId = ctx.SessionId;
            var result = await encMethod.ExecuteAsync(_context);
            if (result != null)
            {
                MTProtoMessage message = new MTProtoMessage();
                message.SessionId = ctx.SessionId;
                message.IsResponse = true;
                message.IsContentRelated = true;
                message.Data = result.TLBytes.ToArray();

                if (await _sessionManager.GetSessionStateAsync(ctx.SessionId)
                    is SessionState session)
                {
                    var bytes = MessagePackSerializer.Serialize(message);
                    _ = _pipe.WriteAsync(session.NodeId.ToString(), bytes);
                }

                Console.WriteLine("-->" + result.ToString());
            }
        }
    }
}

