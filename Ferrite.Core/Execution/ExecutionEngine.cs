// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using System.Collections.Immutable;
using Autofac;
using Autofac.Features.Indexed;
using Ferrite.Core.Execution.Functions;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.slim;
using Ferrite.Utils;

namespace Ferrite.Core.Execution;

public class ExecutionEngine : IExecutionEngine
{
    private readonly IIndex<FunctionKey, ITLFunction> _functions;
    private readonly IMTProtoService _mtproto;
    private readonly IAuthService _auth;
    private readonly ILogger _log;
    private readonly SortedSet<int> _tempMethods = new();
    private readonly SortedSet<int> _unauthorizedMethods = new();

    public ExecutionEngine(IIndex<FunctionKey, ITLFunction> functions, 
        IMTProtoService mtproto, IAuthService auth, ILogger log)
    {
        _functions = functions;
        _mtproto = mtproto;
        _auth = auth;
        _log = log;
        AddUnauthorizedMethods();
        AddTempMethods();
    }
    
    public async ValueTask<TLBytes?> Invoke(TLBytes rpc, TLExecutionContext ctx, int layer = IExecutionEngine.DefaultLayer)
    {
        var keyStatus = await _mtproto.GetKeyStatus(ctx.CurrentAuthKeyId);
        if (ctx.CurrentAuthKeyId != 0 && 
            keyStatus == KeyStatus.TempUnbound &&
            !IsTempKeyAllowed(rpc.Constructor))
        {
            return null;
        }
        if (RequiresAuthorization(rpc.Constructor) && 
            !await _auth.IsAuthorized(ctx.CurrentAuthKeyId))
        {
            return null;
        }
        try
        {
            var func = _functions[new FunctionKey(layer, rpc.Constructor)];
            return await func.Process(rpc, ctx);
        }
        catch (Exception e)
        {
            _log.Error(e, $"#{rpc.Constructor.ToString("x")} is not registered for layer {layer}");
        }

        return null;
    }

    public bool IsImplemented(int constructor, int layer = IExecutionEngine.DefaultLayer)
    {
        try
        {
            var func = _functions[new FunctionKey(layer, constructor)];
            return true;
        }
        catch (Exception e)
        {
            _log.Error(e, $"#{constructor.ToString("x")} is not registered for layer {layer}");
        }

        return false;
    }

    private bool RequiresAuthorization(int constructor)
    {
        return !_unauthorizedMethods.Contains(constructor);
    }
    
    private bool IsTempKeyAllowed(int constructor)
    {
        return _tempMethods.Contains(constructor);
    }
    
    private void AddUnauthorizedMethods()
    {
        _unauthorizedMethods.Add(-1502141361);//auth.sendCode
        _unauthorizedMethods.Add(1056025023);//auth.resendCode
        _unauthorizedMethods.Add(unchecked((int)0x1f040578));//auth.cancelCode
        _unauthorizedMethods.Add(1418342645);//account.getPassword
        _unauthorizedMethods.Add(-779399914);//auth.checkPassword
        _unauthorizedMethods.Add(-2131827673);//auth.signUp
        _unauthorizedMethods.Add(-1126886015);//auth.signIn
        _unauthorizedMethods.Add(-1923962543);//auth.signIn
        _unauthorizedMethods.Add(-1518699091);//auth.importAuthorization
        _unauthorizedMethods.Add(-990308245);//help.getConfig
        _unauthorizedMethods.Add(531836966);//help.getNearestDC
        _unauthorizedMethods.Add(1378703997);//help.getAppUpdate
        _unauthorizedMethods.Add(1375900482);//help.getCDNConfig
        _unauthorizedMethods.Add(1935116200);//help.getCountriesList
        _unauthorizedMethods.Add(-219008246);//langpack.getLangPack
        _unauthorizedMethods.Add(unchecked((int)0x9ab5c58e));//langpack.getLangPackL67
        _unauthorizedMethods.Add(-269862909);//langpack.getStrings
        _unauthorizedMethods.Add(-845657435);//langpack.getDifference
        _unauthorizedMethods.Add(1120311183);//langpack.getLanguages
        _unauthorizedMethods.Add(-2146445955);//langpack.getLanguagesL67
        _unauthorizedMethods.Add(1784243458);//langpack.getLanguage
        _unauthorizedMethods.Add(-1043505495);//InitConnection
        _unauthorizedMethods.Add(-841733627);//auth.bindTempAuthKey
        _unauthorizedMethods.Add(-1188971260);//get_future_salts
        _unauthorizedMethods.Add(unchecked((int)0xbe7e8ef1));//req_pq_multi
        _unauthorizedMethods.Add(unchecked((int)0xd712e4be));//req_dh_params
        _unauthorizedMethods.Add(unchecked((int)0xf5045f1f));//set_client_dh_params
        _unauthorizedMethods.Add(unchecked((int)0x7abe77ec));//ping
        _unauthorizedMethods.Add(unchecked((int)0xf3427b8c));//ping_delay_disconnect
        _unauthorizedMethods.Add(-414113498);//destroy_session
        _unauthorizedMethods.Add(1491380032);//rpc_drop_answer
        _unauthorizedMethods.Add(1658238041);//msgs_ack
        _unauthorizedMethods.Add(2018609336);//initConnection
        _unauthorizedMethods.Add(unchecked((int)0xda9b0d0d));//invokeWithLayer
    }
    private void AddTempMethods()
    {
        _tempMethods.Add(-990308245);//help.getConfig
        _tempMethods.Add(531836966);//help.getNearestDC
        _tempMethods.Add(-841733627);//auth.bindTempAuthKey
    }
}