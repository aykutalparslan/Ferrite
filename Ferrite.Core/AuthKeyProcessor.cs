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
using System.Runtime.InteropServices;
using DotNext;
using Ferrite.Core.Methods;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using Ferrite.TL.slim;
using Ferrite.TL.slim.mtproto;
using Ferrite.Utils;
using MessagePack;

namespace Ferrite.Core;

public class AuthKeyProcessor : IProcessor
{
    private readonly ISessionService _sessionManager;
    private readonly IMessagePipe _pipe;
    private readonly ILogger _log;
    private readonly IApiLayer _api;
    public AuthKeyProcessor(ISessionService sessionManager, IMessagePipe pipe, ILogger log, IApiLayer api)
    {
        _sessionManager = sessionManager;
        _pipe = pipe;
        _log = log;
        _api = api;
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
            if (result == null)
            {
                return;
            }
            MTProtoMessage message = new MTProtoMessage();
            message.SessionId = ctx.SessionId;
            message.IsResponse = true;
            message.IsContentRelated = true;
            message.Data = result.TLBytes.ToArray();
            await _sessionManager.AddAuthSessionAsync(reqPq.Nonce,
                new AuthSessionState() { NodeId = _sessionManager.NodeId, SessionData = ctx.SessionData },
                new ActiveSession(connection));
            message.Nonce = reqPq.Nonce;
            message.MessageType = MTProtoMessageType.Unencrypted;
            var bytes = MessagePackSerializer.Serialize(message);
            await connection.SendAsync(message);

            _log.Information($"{result} sent.");
        }
        else if (input is ReqDhParams reqDhParams &&
            await _sessionManager.GetAuthSessionStateAsync(reqDhParams.Nonce)
            is { } state)
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
        else if (input is SetClientDhParams setClientDhParams &&
           await _sessionManager.GetAuthSessionStateAsync(setClientDhParams.Nonce)
           is { } state2)
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

    public async Task Process(object? sender, EncodedObject input, Queue<EncodedObject> output, TLExecutionContext ctx)
    {
        var constructor = MemoryMarshal.Read<int>(input.AsSpan());
        if (constructor == Constructors.req_pq_multi &&
            sender is MTProtoConnection connection)
        {
            var handler = _api.GetHandler(Constructors.req_pq_multi);
            if (handler != null)
            {
                var result = await handler.Process(input, ctx);
                if (result == null)
                {
                    input.Dispose();
                    return;
                }
                MTProtoMessage message = new MTProtoMessage();
                message.SessionId = ctx.SessionId;
                message.IsResponse = true;
                message.IsContentRelated = true;
                message.Data = result.Value.AsSpan().ToArray();
                var nonce = new req_pq_multi(input.AsSpan()).nonce.ToArray();
                await _sessionManager.AddAuthSessionAsync(nonce,
                    new AuthSessionState() { NodeId = _sessionManager.NodeId, SessionData = ctx.SessionData },
                    new ActiveSession(connection));
                message.Nonce = nonce;
                message.MessageType = MTProtoMessageType.Unencrypted;
                var bytes = MessagePackSerializer.Serialize(message);
                await connection.SendAsync(message);

                _log.Information("Result for req_pq_multi sent.");
            }
            
        }
        else if (constructor == Constructors.req_DH_params)
        {
            var nonce = new req_DH_params(input.AsSpan()).nonce.ToArray();
            var state = await _sessionManager.GetAuthSessionStateAsync(nonce);
            if (state == null)
            {
                input.Dispose();
                return;
            }
            var handler = _api.GetHandler(Constructors.req_DH_params);
            if (handler != null)
            {
                var result = await handler.Process(input, ctx);
                if (result == null)
                {
                    input.Dispose();
                    return;
                }
                ctx.SessionData = state.SessionData;
                MTProtoMessage message = new MTProtoMessage();
                message.SessionId = ctx.SessionId;
                message.IsResponse = true;
                message.IsContentRelated = true;
                message.Data = result.Value.AsSpan().ToArray();
                message.MessageType = MTProtoMessageType.Unencrypted;
                message.Nonce = nonce;
            
                await _sessionManager.UpdateAuthSessionAsync(nonce, new AuthSessionState()
                {
                    NodeId = _sessionManager.NodeId,
                    SessionData = ctx.SessionData
                });
                if (sender != null)
                {
                    await ((MTProtoConnection)sender).SendAsync(message);
                }
                _log.Information("Result for req_DH_params sent.");
            }
        }
        else if (constructor == Constructors.set_client_DH_params)
        {
            var handler = _api.GetHandler(Constructors.set_client_DH_params);
            if (handler != null)
            {
                var nonce = new set_client_DH_params(input.AsSpan()).nonce.ToArray();
                var state = await _sessionManager.GetAuthSessionStateAsync(nonce);
                if (state == null)
                {
                    input.Dispose();
                    return;
                }
                var result = await handler.Process(input, ctx);
                if (result == null)
                {
                    input.Dispose();
                    return;
                }
                ctx.SessionData = state.SessionData;
                MTProtoMessage message = new MTProtoMessage();
                message.SessionId = ctx.SessionId;
                message.IsResponse = true;
                message.IsContentRelated = true;
                message.Data = result.Value.AsSpan().ToArray();
                message.MessageType = MTProtoMessageType.Unencrypted;
                message.Nonce = nonce;
                await _sessionManager.UpdateAuthSessionAsync(nonce, new AuthSessionState()
                {
                    NodeId = _sessionManager.NodeId,
                    SessionData = ctx.SessionData
                });
                if (sender != null)
                {
                    await ((MTProtoConnection)sender).SendAsync(message);
                }
                _log.Information("Result for set_client_DH_params sent.");
            }
        }
        input.Dispose();
    }
}

