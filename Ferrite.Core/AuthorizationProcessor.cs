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
using MessagePack;

namespace Ferrite.Core;

public class AuthorizationProcessor : IProcessor
{
    private readonly ILifetimeScope _scope;
    private readonly ISessionService _sessionManager;
    private readonly IAuthService _auth;
    private readonly IDistributedPipe _pipe;
    private readonly SortedSet<int> _unauthorizedMethods = new();
    public AuthorizationProcessor(ILifetimeScope scope, ISessionService sessionManager,
        IAuthService auth, IDistributedPipe pipe)
    {
        _scope = scope;
        _sessionManager = sessionManager;
        _auth = auth;
        _pipe = pipe;
        AddUnauthorizedMethods();
    }

    private void AddUnauthorizedMethods()
    {
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Auth_SendCode);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Auth_ResendCode);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Account_GetPassword);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Auth_CheckPassword);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Auth_SignUp);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Auth_SignIn);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Auth_ImportAuthorization);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Help_GetConfig);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Help_GetNearestDc);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Help_GetAppUpdate);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Help_GetCdnConfig);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Langpack_GetLangPack);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Langpack_GetStrings);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Langpack_GetDifference);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Langpack_GetLanguages);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.Langpack_GetLanguage);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.InitConnection);
        _unauthorizedMethods.Add(TL.layer139.TLConstructor.JsonObject);
        _unauthorizedMethods.Add(TLConstructor.GetFutureSalts);
        _unauthorizedMethods.Add(2018609336);//initConnection
    }

    public async Task Process(object? sender, ITLObject input, Queue<ITLObject> output, TLExecutionContext ctx)
    {
        bool isAuthorized = await _auth.IsAuthorized(ctx.AuthKeyId);
        if (isAuthorized || _unauthorizedMethods.Contains(input.Constructor))
        {
            output.Enqueue(input);
        }
        else if (input is Message message2)
        {
            if (_unauthorizedMethods.Contains(message2.Body.Constructor))
            {
                output.Enqueue(input);
            }
            else if (message2.Body is TL.layer139.InvokeWithLayer invoke2 &&
                _unauthorizedMethods.Contains(invoke2.Query.Constructor))
            {
                output.Enqueue(input);
            }
            else if (message2.Body is TL.layer139.InvokeAfterMsg invokeAfter &&
                _unauthorizedMethods.Contains(invokeAfter.Query.Constructor))
            {
                output.Enqueue(input);
            }
            else if (message2.Body is TL.layer139.InvokeAfterMsgs invokeAfter2 &&
                _unauthorizedMethods.Contains(invokeAfter2.Query.Constructor))
            {
                output.Enqueue(input);
            }
        }
        else if (input is TL.layer139.InvokeWithLayer invoke &&
            _unauthorizedMethods.Contains(invoke.Query.Constructor))
        {
            output.Enqueue(input);
        }
        else if (input is TL.layer139.InvokeAfterMsg invokeAfter &&
            _unauthorizedMethods.Contains(invokeAfter.Query.Constructor))
        {
            output.Enqueue(input);
        }
        else if (input is TL.layer139.InvokeAfterMsgs invokeAfter2 &&
            _unauthorizedMethods.Contains(invokeAfter2.Query.Constructor))
        {
            output.Enqueue(input);
        }
        else if (input is TL.layer139.InvokeWithLayer invokeWithLayer &&
            _unauthorizedMethods.Contains(invokeWithLayer.Query.Constructor))
        {
            output.Enqueue(input);
        }
        else
        {
            var response = _scope.Resolve<RpcError>();
            response.ErrorCode = 401;
            response.ErrorMessage = "UNAUTHORIZED";
            MTProtoMessage message = new MTProtoMessage();
            message.SessionId = ctx.SessionId;
            message.IsResponse = true;
            message.IsContentRelated = true;
            message.Data = response.TLBytes.ToArray();

            if (await _sessionManager.GetSessionStateAsync(ctx.SessionId)
                is SessionState session)
            {
                var bytes = MessagePackSerializer.Serialize(message);
                _ = _pipe.WriteAsync(session.NodeId.ToString(), bytes);
            }

            Console.WriteLine("-->" + response.ToString());
            return;
        }
    }
}

