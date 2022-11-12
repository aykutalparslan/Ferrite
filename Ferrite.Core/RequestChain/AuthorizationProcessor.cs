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

using System.Buffers;
using Autofac;
using Ferrite.Core.Connection;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using Ferrite.TL.slim;
using Ferrite.Utils;
using Message = Ferrite.TL.mtproto.Message;

namespace Ferrite.Core.RequestChain;

public class AuthorizationProcessor : ILinkedHandler
{
    private readonly ILifetimeScope _scope;
    private readonly ISessionService _sessionManager;
    private readonly IAuthService _auth;
    private readonly IMessagePipe _pipe;
    private readonly ILogger _log;
    private readonly SortedSet<int> _unauthorizedMethods = new();
    public AuthorizationProcessor(ILifetimeScope scope, ISessionService sessionManager,
        IAuthService auth, IMessagePipe pipe, ILogger log)
    {
        _scope = scope;
        _sessionManager = sessionManager;
        _auth = auth;
        _pipe = pipe;
        _log = log;
        AddUnauthorizedMethods();
    }
    
    public ILinkedHandler SetNext(ILinkedHandler value)
    {
        Next = value;
        return Next;
    }

    private void AddUnauthorizedMethods()
    {
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Auth_SendCode);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Auth_ResendCode);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Account_GetPassword);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Auth_CheckPassword);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Auth_SignUp);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Auth_SignIn);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Auth_ImportAuthorization);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Help_GetConfig);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Help_GetNearestDc);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Help_GetAppUpdate);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Help_GetCdnConfig);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Help_GetCountriesList);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Langpack_GetLangPack);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Langpack_GetLangPack67);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Langpack_GetStrings);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Langpack_GetDifference);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Langpack_GetLanguages);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Langpack_GetLanguagesL67);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Langpack_GetLanguage);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.InitConnection);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.JsonObject);
        _unauthorizedMethods.Add(TL.currentLayer.TLConstructor.Auth_BindTempAuthKey);
        _unauthorizedMethods.Add(TLConstructor.GetFutureSalts);
        _unauthorizedMethods.Add(TLConstructor.DestroySession);
        _unauthorizedMethods.Add(TLConstructor.RpcDropAnswer);
        _unauthorizedMethods.Add(TLConstructor.MsgsAck);
        _unauthorizedMethods.Add(2018609336);//initConnection
    }

    public ILinkedHandler? Next { get; set; }

    public async ValueTask Process(object? sender, ITLObject input, TLExecutionContext ctx)
    {
        bool isAuthorized = await _auth.IsAuthorized(ctx.PermAuthKeyId!=0 ? ctx.PermAuthKeyId : ctx.AuthKeyId);
        if (isAuthorized || _unauthorizedMethods.Contains(input.Constructor))
        {
            if (Next != null) await Next.Process(sender, input, ctx);
        }
        else if (input is Message message2)
        {
            if (_unauthorizedMethods.Contains(message2.Body.Constructor))
            {
                if (Next != null) await Next.Process(sender, input, ctx);
            }
            else if (message2.Body is TL.currentLayer.InvokeWithLayer invoke2 &&
                _unauthorizedMethods.Contains(invoke2.Query.Constructor))
            {
                if (Next != null) await Next.Process(sender, input, ctx);
            }
            else if (message2.Body is TL.currentLayer.InvokeAfterMsg invokeAfter &&
                _unauthorizedMethods.Contains(invokeAfter.Query.Constructor))
            {
                if (Next != null) await Next.Process(sender, input, ctx);
            }
            else if (message2.Body is TL.currentLayer.InvokeAfterMsgs invokeAfter2 &&
                _unauthorizedMethods.Contains(invokeAfter2.Query.Constructor))
            {
                if (Next != null) await Next.Process(sender, input, ctx);
            }
        }
        else if (input is TL.currentLayer.InvokeWithLayer invoke &&
            _unauthorizedMethods.Contains(invoke.Query.Constructor))
        {
            if (Next != null) await Next.Process(sender, input, ctx);
        }
        else if (input is TL.currentLayer.InvokeAfterMsg invokeAfter &&
            _unauthorizedMethods.Contains(invokeAfter.Query.Constructor))
        {
            if (Next != null) await Next.Process(sender, input, ctx);
        }
        else if (input is TL.currentLayer.InvokeAfterMsgs invokeAfter2 &&
            _unauthorizedMethods.Contains(invokeAfter2.Query.Constructor))
        {
            if (Next != null) await Next.Process(sender, input, ctx);
        }
        else if (input is TL.currentLayer.InvokeWithLayer invokeWithLayer &&
            _unauthorizedMethods.Contains(invokeWithLayer.Query.Constructor))
        {
            if (Next != null) await Next.Process(sender, input, ctx);
        }
        else
        {
            if (ctx.AuthKeyId != 0)
            {
                _log.Debug($"😳 {input} was not processed due to AuthKeyId: " +
                           $"{(ctx.PermAuthKeyId!=0 ? ctx.PermAuthKeyId : ctx.AuthKeyId)} not being logged in");
            }
            var response = _scope.Resolve<RpcError>();
            response.ErrorCode = 401;
            response.ErrorMessage = "UNAUTHORIZED";
            MTProtoMessage message = new Services.MTProtoMessage();
            message.SessionId = ctx.SessionId;
            message.IsResponse = true;
            message.IsContentRelated = true;
            message.Data = response.TLBytes.ToArray();

            if (sender != null)
            {
                await ((MTProtoConnection)sender).SendAsync(message);
            }

            Console.WriteLine("-->" + response.ToString());
        }
    }

    public async ValueTask Process(object? sender, TLBytes input, TLExecutionContext ctx)
    {
        if (Next != null) await Next.Process(sender, input, ctx);
    }
}

