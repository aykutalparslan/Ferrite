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
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using Ferrite.Utils;
using MessagePack;

namespace Ferrite.Core;

public class AuthKeyProcessor : IProcessor
{
    private readonly ISessionService _sessionManager;
    private readonly IDistributedPipe _pipe;
    private readonly ILogger _log;
    public AuthKeyProcessor(ISessionService sessionManager, IDistributedPipe pipe, ILogger log)
    {
        _sessionManager = sessionManager;
        _pipe = pipe;
        _log = log;
    }

    public async Task Process(object? sender, ITLObject input, Queue<ITLObject> output, TLExecutionContext ctx)
    {
        _log.Information($"{input} received.");
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
            if (result == null)
            {
                return;
            }
            MTProtoMessage message = new MTProtoMessage();
            message.SessionId = ctx.SessionId;
            message.IsResponse = true;
            message.IsContentRelated = true;
            message.Data = result.TLBytes.ToArray();
            message.Nonce = reqPq.Nonce;
            message.MessageType = MTProtoMessageType.Unencrypted;
            var bytes = MessagePackSerializer.Serialize(message);
            await connection.SendAsync(message);

            _log.Information($"{result} sent.");
        }
        else if (input is ReqDhParams reqDhParams)
        {
            var result = await reqDhParams.ExecuteAsync(ctx);
            MTProtoMessage message = new MTProtoMessage();
            message.SessionId = ctx.SessionId;
            message.IsResponse = true;
            message.IsContentRelated = true;
            message.Data = result.TLBytes.ToArray();
            message.MessageType = MTProtoMessageType.Unencrypted;
            message.Nonce = reqDhParams.Nonce;
            
            await _sessionManager.UpdateAuthSessionAsync(reqDhParams.Nonce, new AuthSessionState()
            {
                NodeId = _sessionManager.NodeId,
                SessionData = ctx.SessionData
            });
            if (sender != null)
            {
                await ((MTProtoConnection)sender).SendAsync(message);
            }
            _log.Information($"{result} sent.");
        }
        else if (input is SetClientDhParams setClientDhParams)
        {
            var result = await setClientDhParams.ExecuteAsync(ctx);
            MTProtoMessage message = new MTProtoMessage();
            message.SessionId = ctx.SessionId;
            message.IsResponse = true;
            message.IsContentRelated = true;
            message.Data = result.TLBytes.ToArray();
            message.MessageType = MTProtoMessageType.Unencrypted;
            message.Nonce = setClientDhParams.Nonce;
            //var bytes = MessagePackSerializer.Serialize(message);
            await _sessionManager.UpdateAuthSessionAsync(setClientDhParams.Nonce, new AuthSessionState()
            {
                NodeId = _sessionManager.NodeId,
                SessionData = ctx.SessionData
            });
            if (sender != null)
            {
                await ((MTProtoConnection)sender).SendAsync(message);
            }
            _log.Information($"{result} sent.");
        }
    }
}

