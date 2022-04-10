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

public class AuthKeyProcessor : IProcessor
{
    private readonly ISessionManager _sessionManager;
    private readonly IDistributedPipe _pipe;
    public AuthKeyProcessor(ISessionManager sessionManager, IDistributedPipe pipe)
    {
        _sessionManager = sessionManager;
        _pipe = pipe;
    }

    public async Task Process(object? sender, ITLObject input, Queue<ITLObject> output, TLExecutionContext ctx)
    {
        if (ctx.AuthKeyId != 0)
        {
            output.Enqueue(input);
            return;
        }
        if (input.Constructor != TLConstructor.ReqPqMulti &&
            input.Constructor != TLConstructor.ReqDhParams &&
            input.Constructor != TLConstructor.SetClientDhParams)
        {
            return;
        }

        if (input is ReqPqMulti reqPq &&
            sender is MTProtoConnection connection)
        {
            var result = await reqPq.ExecuteAsync(ctx);
            MTProtoMessage message = new MTProtoMessage();
            message.SessionId = ctx.SessionId;
            message.IsResponse = true;
            message.IsContentRelated = true;
            message.Data = result.TLBytes.ToArray();
            await _sessionManager.AddAuthSessionAsync(reqPq.Nonce,
                new AuthSessionState() { NodeId = _sessionManager.NodeId, SessionData = ctx.SessionData },
                new MTPtotoSession(connection));
            message.Nonce = reqPq.Nonce;
            message.MessageType = MTProtoMessageType.Unencrypted;
            var bytes = MessagePackSerializer.Serialize(message);
            _ = _pipe.WriteAsync(_sessionManager.NodeId.ToString(), bytes);

            Console.WriteLine("-->" + result.ToString());
        }
        else if (input is ReqDhParams reqDhParams &&
            await _sessionManager.GetAuthSessionStateAsync(reqDhParams.Nonce)
            is AuthSessionState state)
        {
            ctx.SessionData = state.SessionData;
            var result = await reqDhParams.ExecuteAsync(ctx);
            MTProtoMessage message = new MTProtoMessage();
            message.SessionId = ctx.SessionId;
            message.IsResponse = true;
            message.IsContentRelated = true;
            message.Data = result.TLBytes.ToArray();
            message.MessageType = MTProtoMessageType.Unencrypted;
            message.Nonce = reqDhParams.Nonce;
            var bytes = MessagePackSerializer.Serialize(message);
            _ = _pipe.WriteAsync(_sessionManager.NodeId.ToString(), bytes);
            await _sessionManager.UpdateAuthSessionAsync(reqDhParams.Nonce, new AuthSessionState()
            {
                NodeId = _sessionManager.NodeId,
                SessionData = ctx.SessionData
            });

            Console.WriteLine("-->" + result.ToString());
        }
        else if (input is SetClientDhParams setClientDhParams &&
           await _sessionManager.GetAuthSessionStateAsync(setClientDhParams.Nonce)
           is AuthSessionState state2)
        {
            ctx.SessionData = state2.SessionData;
            var result = await setClientDhParams.ExecuteAsync(ctx);
            MTProtoMessage message = new MTProtoMessage();
            message.SessionId = ctx.SessionId;
            message.IsResponse = true;
            message.IsContentRelated = true;
            message.Data = result.TLBytes.ToArray();
            message.MessageType = MTProtoMessageType.Unencrypted;
            message.Nonce = setClientDhParams.Nonce;
            var bytes = MessagePackSerializer.Serialize(message);
            _ = _pipe.WriteAsync(_sessionManager.NodeId.ToString(), bytes);
            await _sessionManager.UpdateAuthSessionAsync(setClientDhParams.Nonce, new AuthSessionState()
            {
                NodeId = _sessionManager.NodeId,
                SessionData = ctx.SessionData
            });

            Console.WriteLine("-->" + result.ToString());
        }
    }
}

