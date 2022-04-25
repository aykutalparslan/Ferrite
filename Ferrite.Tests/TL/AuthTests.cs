//
//  Project Ferrite is an Implementation of the Telegram Server API
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Ferrite.Core;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Account;
using Ferrite.Data.Auth;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.layer139;
using Ferrite.TL.layer139.auth;
using Ferrite.TL.mtproto;
using Ferrite.Transport;
using Ferrite.Utils;
using MessagePack;
using Xunit;

namespace Ferrite.Tests.TL;

public class AuthTests
{
    [Fact]
    public async Task SendCode_Returns_SentCode()
    {
        var container = BuildIoCContainer();
        var factory = container.Resolve<TLObjectFactory>();
        var sendCode = factory.Resolve<SendCode>();
        var result = await sendCode.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<SentCodeImpl>(rslt.Result);
        var sntCode = (SentCodeImpl)rslt.Result;
        Assert.IsType<SentCodeTypeSmsImpl>(sntCode.Type);
        Assert.NotNull(sntCode.NextType);
        Assert.Equal(60, sntCode.Timeout);
        Assert.Equal("acabadef", sntCode.PhoneCodeHash);
    }
    [Fact]
    public async Task SignIn_Returns_SignUpRequired()
    {
        var container = BuildIoCContainer();
        var factory = container.BeginLifetimeScope().Resolve<TLObjectFactory>();
        var signIn = factory.Resolve<SignIn>();
        signIn.PhoneCode = "12345";
        signIn.PhoneCodeHash = "acabadef";
        signIn.PhoneNumber = "5554443322";
        var result = await signIn.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<AuthorizationSignUpRequiredImpl>(rslt.Result);
    }
    [Fact]
    public async Task SignIn_Returns_Authorization()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        var authService = scope.Resolve<IAuthService>();
        await authService.SignUp(0,"5554443322", "acabadef", "a", "b");
        
        var signIn = factory.Resolve<SignIn>();
        signIn.PhoneCode = "12345";
        signIn.PhoneCodeHash = "acabadef";
        signIn.PhoneNumber = "5554443322";
        var result = await signIn.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1224
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1224, rslt.ReqMsgId);
        Assert.IsType<Ferrite.TL.layer139.auth.AuthorizationImpl>(rslt.Result);
        var auth = (Ferrite.TL.layer139.auth.AuthorizationImpl)rslt.Result;
        Assert.IsType<UserImpl>(auth.User);
        var user = (UserImpl)auth.User;
        Assert.NotEqual(0, user.Id);
        Assert.Equal("a", user.FirstName);
        Assert.Equal("b", user.LastName);
        Assert.Equal("5554443322", user.Phone);
        Assert.IsType<UserStatusEmptyImpl>(user.Status);
        Assert.IsType<UserProfilePhotoEmptyImpl>(user.Photo);
    }
    [Fact]
    public async Task SignUp_Returns_SignUpRequired()
    {
        var container = BuildIoCContainer();
        var factory = container.Resolve<TLObjectFactory>();
        var signUp = factory.Resolve<SignUp>();
        signUp.PhoneCodeHash = "xxx";
        signUp.PhoneNumber = "5554443322";
        signUp.FirstName = "a";
        signUp.LastName = "b";
        var result = await signUp.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<AuthorizationSignUpRequiredImpl>(rslt.Result);
    }
    [Fact]
    public async Task SignUp_Returns_Authorization()
    {
        var container = BuildIoCContainer();
        var factory = container.Resolve<TLObjectFactory>();
        var signUp = factory.Resolve<SignUp>();
        signUp.PhoneCodeHash = "acabadef";
        signUp.PhoneNumber = "5554443322";
        signUp.FirstName = "a";
        signUp.LastName = "b";
        var result = await signUp.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<Ferrite.TL.layer139.auth.AuthorizationImpl>(rslt.Result);
        var auth = (Ferrite.TL.layer139.auth.AuthorizationImpl)rslt.Result;
        Assert.IsType<UserImpl>(auth.User);
        var user = (UserImpl)auth.User;
        Assert.NotEqual(0, user.Id);
        Assert.Equal("a", user.FirstName);
        Assert.Equal("b", user.LastName);
        Assert.Equal("5554443322", user.Phone);
        Assert.IsType<UserStatusEmptyImpl>(user.Status);
        Assert.IsType<UserProfilePhotoEmptyImpl>(user.Photo);
    }
    [Fact]
    public async Task Logout_Returns_LoggedOut()
    {
        var container = BuildIoCContainer();
        var factory = container.BeginLifetimeScope().Resolve<TLObjectFactory>();
        var logout = factory.Resolve<LogOut>();
        var ctx = new TLExecutionContext(new Dictionary<string, object>());
        ctx.AuthKeyId = 111;
        ctx.MessageId = 1223;
        var result = await logout.ExecuteAsync(ctx);
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<LoggedOutImpl>(rslt.Result);
    }
    [Fact]
    public async Task Logout_Returns_RpcError()
    {
        var container = BuildIoCContainer();
        var factory = container.BeginLifetimeScope().Resolve<TLObjectFactory>();
        var logout = factory.Resolve<LogOut>();
        var ctx = new TLExecutionContext(new Dictionary<string, object>());
        ctx.AuthKeyId = 0;
        ctx.MessageId = 1223;
        var result = await logout.ExecuteAsync(ctx);
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<RpcError>(rslt.Result);
    }
    [Fact]
    public async Task ResetAuthorizations_Returns_True()
    {
        var container = BuildIoCContainer();
        var factory = container.BeginLifetimeScope().Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<ResetAuthorizations>();
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<BoolTrue>(rslt.Result);
    }
    [Fact]
    public async Task ResetAuthorizations_Returns_False()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        FakeAuthService authService = (FakeAuthService)scope.Resolve<IAuthService>();
        authService.ResetAuthorizationsResult = false;
        var rpc = factory.Resolve<ResetAuthorizations>();
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<BoolFalse>(rslt.Result);
    }
    [Fact]
    public async Task ExportAuthorization_Returns_ExportedAuthorization()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<ExportAuthorization>();
        rpc.DcId = 1;
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<ExportedAuthorizationImpl>(rslt.Result);
        var exported = (ExportedAuthorizationImpl)rslt.Result;
        Assert.NotEqual(0, exported.Id);
        Assert.NotNull(exported.Bytes);
    }
    [Fact]
    public async Task ImportAuthorization_Returns_Authorization()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<ImportAuthorization>();
        rpc.Id = 112233;
        rpc.Bytes = new byte[] { 1, 2, 3, 4, 5, 6 };
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<Ferrite.TL.layer139.auth.AuthorizationImpl>(rslt.Result);
        var auth = (Ferrite.TL.layer139.auth.AuthorizationImpl)rslt.Result;
        Assert.IsType<UserImpl>(auth.User);
        var user = (UserImpl)auth.User;
        Assert.NotEqual(0, user.Id);
        Assert.Equal("a", user.FirstName);
        Assert.Equal("b", user.LastName);
        Assert.Equal("5554443322", user.Phone);
        Assert.IsType<UserStatusEmptyImpl>(user.Status);
        Assert.IsType<UserProfilePhotoEmptyImpl>(user.Photo);
    }
    [Fact]
    public async Task ImportAuthorization_Returns_AuthBytesInvalid()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        FakeAuthService authService = (FakeAuthService)scope.Resolve<IAuthService>();
        authService.ImportAuthorizationFailed = true;
        var rpc = factory.Resolve<ImportAuthorization>();
        rpc.Id = 112233;
        rpc.Bytes = new byte[] { 1, 2, 3, 4, 5, 6 };
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<RpcError>(rslt.Result);
        var err = (RpcError)rslt.Result;
        Assert.Equal(400, err.ErrorCode);
        Assert.Equal("AUTH_BYTES_INVALID", err.ErrorMessage);
    }
    [Fact]
    public async Task BindTempAuthKey_Returns_True()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<BindTempAuthKey>();
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<BoolTrue>(rslt.Result);
    }
    [Fact]
    public async Task BindTempAuthKey_Returns_False()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        FakeAuthService authService = (FakeAuthService)scope.Resolve<IAuthService>();
        authService.BindTempAuthKeyFailed = true;
        var rpc = factory.Resolve<BindTempAuthKey>();
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<BoolFalse>(rslt.Result);
    }
    [Fact]
    public async Task ImportBotAuthorization_Returns_Authorization()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<ImportBotAuthorization>();
        rpc.ApiId = 1;
        rpc.ApiHash = "a";
        rpc.BotAuthToken = "bot1";
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<Ferrite.TL.layer139.auth.AuthorizationImpl>(rslt.Result);
        var auth = (Ferrite.TL.layer139.auth.AuthorizationImpl)rslt.Result;
        Assert.IsType<UserImpl>(auth.User);
        var user = (UserImpl)auth.User;
        Assert.NotEqual(0, user.Id);
        Assert.Equal("a", user.FirstName);
        Assert.Equal("b", user.LastName);
        Assert.Equal("5554443322", user.Phone);
        Assert.IsType<UserStatusEmptyImpl>(user.Status);
        Assert.IsType<UserProfilePhotoEmptyImpl>(user.Photo);
    }
    [Fact]
    public async Task ImportBotAuthorization_Returns_SignUpRequired()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        FakeAuthService authService = (FakeAuthService)scope.Resolve<IAuthService>();
        authService.ImportAuthorizationFailed = true;
        var rpc = factory.Resolve<ImportBotAuthorization>();
        rpc.ApiId = 1;
        rpc.ApiHash = "a";
        rpc.BotAuthToken = "bot1";
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<AuthorizationSignUpRequiredImpl>(rslt.Result);
    }
    [Fact]
    public async Task CheckPassword_Returns_Authorization()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<CheckPassword>();
        var password = factory.Resolve<InputCheckPasswordSRPImpl>();
        password.A = new byte[] { 1, 2, 3, 4, 5, 6 };
        password.M1 = new byte[] { 2, 3, 4, 5, 6, 7 };
        password.SrpId = 5555;
        rpc.Password = password;
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<Ferrite.TL.layer139.auth.AuthorizationImpl>(rslt.Result);
        var auth = (Ferrite.TL.layer139.auth.AuthorizationImpl)rslt.Result;
        Assert.IsType<UserImpl>(auth.User);
        var user = (UserImpl)auth.User;
        Assert.NotEqual(0, user.Id);
        Assert.Equal("a", user.FirstName);
        Assert.Equal("b", user.LastName);
        Assert.Equal("5554443322", user.Phone);
        Assert.IsType<UserStatusEmptyImpl>(user.Status);
        Assert.IsType<UserProfilePhotoEmptyImpl>(user.Photo);
    }
    [Fact]
    public async Task CheckPassword_Returns_SignUpRequired()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<CheckPassword>();
        var password = factory.Resolve<InputCheckPasswordEmptyImpl>();
        rpc.Password = password;
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<AuthorizationSignUpRequiredImpl>(rslt.Result);
    }
    [Fact]
    public async Task RequestPasswordRecovery_Returns_PasswordRecovery()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<RequestPasswordRecovery>();
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<PasswordRecoveryImpl>(rslt.Result);
        var rec = (PasswordRecoveryImpl)rslt.Result;
        Assert.Equal("a@b.com", rec.EmailPattern);
    }
    [Fact]
    public async Task RecoverPassword_Returns_Authorization()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<RecoverPassword>();
        rpc.Code = "1234";
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<Ferrite.TL.layer139.auth.AuthorizationImpl>(rslt.Result);
        var auth = (Ferrite.TL.layer139.auth.AuthorizationImpl)rslt.Result;
        Assert.IsType<UserImpl>(auth.User);
        var user = (UserImpl)auth.User;
        Assert.NotEqual(0, user.Id);
        Assert.Equal("a", user.FirstName);
        Assert.Equal("b", user.LastName);
        Assert.Equal("5554443322", user.Phone);
        Assert.IsType<UserStatusEmptyImpl>(user.Status);
        Assert.IsType<UserProfilePhotoEmptyImpl>(user.Photo);
    }
    [Fact]
    public async Task RecoverPassword_Returns_SignUpRequired()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        FakeAuthService authService = (FakeAuthService)scope.Resolve<IAuthService>();
        authService.RecoverPasswordFailed = true;
        var rpc = factory.Resolve<RecoverPassword>();
        rpc.Code = "1234";
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<AuthorizationSignUpRequiredImpl>(rslt.Result);
    }
    [Fact]
    public async Task ResendCode_Returns_SentCode()
    {
        var container = BuildIoCContainer();
        var factory = container.Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<ResendCode>();
        rpc.PhoneNumber = "5554443322";
        rpc.PhoneCodeHash = "acabadef";
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<SentCodeImpl>(rslt.Result);
        var sntCode = (SentCodeImpl)rslt.Result;
        Assert.IsType<SentCodeTypeSmsImpl>(sntCode.Type);
        Assert.NotNull(sntCode.NextType);
        Assert.Equal(60, sntCode.Timeout);
        Assert.Equal("acabadef", sntCode.PhoneCodeHash);
    }
    [Fact]
    public async Task ResendCode_Returns_Error()
    {
        var container = BuildIoCContainer();
        var factory = container.Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<ResendCode>();
        rpc.PhoneNumber = "5554443322";
        rpc.PhoneCodeHash = "x";
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<RpcError>(rslt.Result);
    }
    [Fact]
    public async Task CancelCode_Returns_True()
    {
        var container = BuildIoCContainer();
        var factory = container.BeginLifetimeScope().Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<CancelCode>();
        rpc.PhoneNumber = "5554443322";
        rpc.PhoneCodeHash = "acabadef";
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<BoolTrue>(rslt.Result);
    }
    [Fact]
    public async Task CancelCode_Returns_False()
    {
        var container = BuildIoCContainer();
        var factory = container.BeginLifetimeScope().Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<CancelCode>();
        rpc.PhoneNumber = "5554443322";
        rpc.PhoneCodeHash = "xxx";
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<BoolFalse>(rslt.Result);
    }
    [Fact]
    public async Task DropTempAuthKeys_Returns_True()
    {
        var container = BuildIoCContainer();
        var factory = container.BeginLifetimeScope().Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<DropTempAuthKeys>();
        rpc.ExceptAuthKeys = new VectorOfLong();
        rpc.ExceptAuthKeys.Add(777);
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<BoolTrue>(rslt.Result);
    }
    [Fact]
    public async Task DropTempAuthKeys_Returns_False()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        FakeAuthService authService = (FakeAuthService)scope.Resolve<IAuthService>();
        authService.DropTempAuthKeysFailed = true;
        var rpc = factory.Resolve<DropTempAuthKeys>();
        rpc.ExceptAuthKeys = new VectorOfLong();
        rpc.ExceptAuthKeys.Add(777);
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<BoolFalse>(rslt.Result);
    }
    [Fact]
    public async Task ExportLoginToken_Returns_LoginToken()
    {
        var container = BuildIoCContainer();
        var factory = container.BeginLifetimeScope().Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<ExportLoginToken>();
        rpc.ApiId = 1;
        rpc.ApiHash = "a";
        rpc.ExceptIds = new VectorOfLong();
        rpc.ExceptIds.Add(9876);
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<LoginTokenImpl>(rslt.Result);
        var token = (LoginTokenImpl)rslt.Result;
        Assert.NotNull(token.Token);
        Assert.True(token.Expires>0);
    }
    [Fact]
    public async Task ExportLoginToken_Returns_LoginTokenMigrateTo()
    {
        var container = BuildIoCContainer();
        var factory = container.BeginLifetimeScope().Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<ExportLoginToken>();
        rpc.ApiId = 1;
        rpc.ApiHash = "a";
        rpc.ExceptIds = new VectorOfLong();
        rpc.ExceptIds.Add(9876);
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<LoginTokenMigrateToImpl>(rslt.Result);
        var token = (LoginTokenMigrateToImpl)rslt.Result;
        Assert.NotNull(token.Token);
        Assert.True(token.DcId > 0);
    }
    [Fact]
    public async Task ExportLoginToken_Returns_LoginTokenSuccess()
    {
        var container = BuildIoCContainer();
        var factory = container.BeginLifetimeScope().Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<ExportLoginToken>();
        rpc.ApiId = 1;
        rpc.ApiHash = "a";
        rpc.ExceptIds = new VectorOfLong();
        rpc.ExceptIds.Add(9876);
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<LoginTokenSuccessImpl>(rslt.Result);
        var token = (LoginTokenSuccessImpl)rslt.Result;
        Assert.IsType<Ferrite.TL.layer139.auth.AuthorizationImpl>(token.Authorization);
        var auth = (Ferrite.TL.layer139.auth.AuthorizationImpl)token.Authorization;
        Assert.IsType<UserImpl>(auth.User);
        var user = (UserImpl)auth.User;
        Assert.NotEqual(0, user.Id);
        Assert.Equal("a", user.FirstName);
        Assert.Equal("b", user.LastName);
        Assert.Equal("5554443322", user.Phone);
        Assert.IsType<UserStatusEmptyImpl>(user.Status);
        Assert.IsType<UserProfilePhotoEmptyImpl>(user.Photo);
    }
    [Fact]
    public async Task ImportLoginToken_Returns_LoginToken()
    {
        var container = BuildIoCContainer();
        var factory = container.BeginLifetimeScope().Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<ImportLoginToken>();
        rpc.Token = new byte[] { 1, 2, 3 };
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<LoginTokenImpl>(rslt.Result);
        var token = (LoginTokenImpl)rslt.Result;
        Assert.NotNull(token.Token);
        Assert.True(token.Expires > 0);
    }
    [Fact]
    public async Task ImportLoginToken_Returns_LoginTokenMigrateTo()
    {
        var container = BuildIoCContainer();
        var factory = container.BeginLifetimeScope().Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<ImportLoginToken>();
        rpc.Token = new byte[] { 1, 2, 3 };
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<LoginTokenMigrateToImpl>(rslt.Result);
        var token = (LoginTokenMigrateToImpl)rslt.Result;
        Assert.NotNull(token.Token);
        Assert.True(token.DcId > 0);
    }
    [Fact]
    public async Task ImportLoginToken_Returns_LoginTokenSuccess()
    {
        var container = BuildIoCContainer();
        var factory = container.BeginLifetimeScope().Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<ImportLoginToken>();
        rpc.Token = new byte[] { 1, 2, 3 };
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<LoginTokenSuccessImpl>(rslt.Result);
        var token = (LoginTokenSuccessImpl)rslt.Result;
        Assert.IsType<Ferrite.TL.layer139.auth.AuthorizationImpl>(token.Authorization);
        var auth = (Ferrite.TL.layer139.auth.AuthorizationImpl)token.Authorization;
        Assert.IsType<UserImpl>(auth.User);
        var user = (UserImpl)auth.User;
        Assert.NotEqual(0, user.Id);
        Assert.Equal("a", user.FirstName);
        Assert.Equal("b", user.LastName);
        Assert.Equal("5554443322", user.Phone);
        Assert.IsType<UserStatusEmptyImpl>(user.Status);
        Assert.IsType<UserProfilePhotoEmptyImpl>(user.Photo);
    }
    [Fact]
    public async Task AcceptLoginToken_Returns_Authorization()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<AcceptLoginToken>();
        rpc.Token = new byte[] { 1, 2, 3 };
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<Ferrite.TL.layer139.AuthorizationImpl>(rslt.Result);
        var auth = (Ferrite.TL.layer139.AuthorizationImpl)rslt.Result;
        Assert.True(auth.Hash > 0);
        Assert.NotNull(auth.DeviceModel);
        Assert.NotNull(auth.Platform);
        Assert.NotNull(auth.SystemVersion);
        Assert.True(auth.ApiId > 0);
        Assert.NotNull(auth.AppName);
        Assert.NotNull(auth.AppVersion);
        Assert.True(auth.DateCreated > 0);
        Assert.True(auth.DateActive > 0);
        Assert.True(IPAddress.TryParse(auth.Ip, out var a));
        Assert.NotNull(auth.Country);
        Assert.NotNull(auth.Region);
    }
    [Fact]
    public async Task CheckRecoveryPassword_Returns_True()
    {
        var container = BuildIoCContainer();
        var factory = container.BeginLifetimeScope().Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<CheckRecoveryPassword>();
        rpc.Code = "1234";
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<BoolTrue>(rslt.Result);
    }
    [Fact]
    public async Task CheckRecoveryPassword_Returns_False()
    {
        var scope = BuildIoCContainer().BeginLifetimeScope();
        var factory = scope.Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<CheckRecoveryPassword>();
        rpc.Code = "1111";
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<BoolFalse>(rslt.Result);
    }

    private IContainer BuildIoCContainer()
    {
        var tl = Assembly.Load("Ferrite.TL");
        var builder = new ContainerBuilder();
        builder.RegisterType<FakeTime>().As<IMTProtoTime>().SingleInstance();
        builder.RegisterType<FakeRandom>().As<IRandomGenerator>();
        builder.RegisterType<KeyProvider>().As<IKeyProvider>();
        builder.RegisterType<FakeAuthService>().As<IAuthService>().InstancePerLifetimeScope();
        builder.RegisterAssemblyTypes(tl)
            .Where(t => t.Namespace == "Ferrite.TL.mtproto")
            .AsSelf();
        builder.RegisterAssemblyTypes(tl)
            .Where(t => t.Namespace == "Ferrite.TL")
            .AsSelf();
        builder.RegisterAssemblyOpenGenericTypes(tl)
            .Where(t => t.Namespace == "Ferrite.TL")
            .AsSelf();
        builder.RegisterAssemblyTypes(tl)
            .Where(t => t.Namespace != null && t.Namespace.StartsWith("Ferrite.TL.layer139"))
            .AsSelf();
        builder.Register(_ => new Int128());
        builder.Register(_ => new Int256());
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterType<FakeCassandra>().As<IPersistentStore>().SingleInstance();
        builder.RegisterType<FakeRedis>().As<IDistributedStore>().SingleInstance();
        builder.RegisterType<FakeLogger>().As<ILogger>().SingleInstance();
        builder.RegisterType<FakeDistributedPipe>().As<IDistributedPipe>().SingleInstance();

        var container = builder.Build();

        return container;
    }
}
class FakeAuthService : IAuthService
{
    bool _signupComplete = false;
    private string _firstName = default!;
    private string _lastName = default!;
    public bool ResetAuthorizationsResult = true;
    public bool ImportAuthorizationFailed = false;
    internal bool RecoverPasswordFailed;
    internal bool DropTempAuthKeysFailed;

    public bool BindTempAuthKeyFailed { get; internal set; }

    public Task<Data.Auth.Authorization> AcceptLoginToken(byte[] token)
    {
        throw new NotImplementedException();
    }

    public Task<bool> BindTempAuthKey(long authKeyId, long permAuthKeyId, int expiresAt)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> CancelCode(string phoneNumber, string phoneCodeHash)
    {
        return phoneCodeHash == "acabadef";
    }

    public Task<Data.Auth.Authorization> CheckPassword(bool empty, long srpId, byte[] A, byte[] M1)
    {
        throw new NotImplementedException();
    }

    public Task<bool> CheckRecoveryPassword(string code)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DropTempAuthKeys(ICollection<long> exceptAuthKeys)
    {
        throw new NotImplementedException();
    }

    public async Task<Data.Auth.ExportedAuthorization> ExportAuthorization(long authKeyId, int dcId)
    {
        return new Data.Auth.ExportedAuthorization()
        {
            Id = 111,
            Bytes = new byte[] {1,2,3}
        };
    }

    public Task<Data.Auth.LoginToken> ExportLoginToken(int apiId, string apiHash, ICollection<long> exceptIds)
    {
        throw new NotImplementedException();
    }

    public async Task<Data.Auth.Authorization> ImportAuthorization(long user_id, long auth_key_id, byte[] bytes)
    {
        if (ImportAuthorizationFailed)
        {
            return new Data.Auth.Authorization()
            {
                AuthorizationType = AuthorizationType.AuthBytesInvalid
            };
        }
        else
        {
            return new Data.Auth.Authorization()
            {
                AuthorizationType = AuthorizationType.Authorization,
                User = new Data.User()
                {
                    Id = 123,
                    FirstName = "a",
                    LastName = "b",
                    Phone = "5554443322",
                    Status = Data.UserStatus.Empty,
                    Self = true,
                    Photo = new Data.UserProfilePhoto()
                    {
                        Empty = true
                    }
                }
            };
        }
    }

    public Task<Data.Auth.Authorization> ImportBotAuthorization(int apiId, string apiHash, string botAuthToken)
    {
        throw new NotImplementedException();
    }

    public Task<Data.Auth.LoginToken> ImportLoginToken(byte[] token)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsAuthorized(long authKeyId)
    {
        throw new NotImplementedException();
    }

    public async Task<Data.Auth.LoggedOut?> LogOut(long authKeyId)
    {
        if(authKeyId == 0)
        {
            return null;
        }
        return new Data.Auth.LoggedOut()
        {
            FutureAuthToken = new byte[] { 1, 2, 3 }
        };
    }

    public Task<Data.Auth.Authorization> RecoverPassword(string code, PasswordInputSettings newSettings)
    {
        throw new NotImplementedException();
    }

    public Task<string> RequestPasswordRecovery()
    {
        throw new NotImplementedException();
    }

    public async Task<Data.Auth.SentCode> ResendCode(string phoneNumber, string phoneCodeHash)
    {
        if(phoneCodeHash!= "acabadef")
        {
            return null;
        }
        return new Data.Auth.SentCode()
        {
            CodeType = Data.Auth.SentCodeType.Sms,
            NextType = Data.Auth.SentCodeType.Sms,
            CodeLength = 5,
            Timeout = 60,
            PhoneCodeHash = "acabadef"
        };
    }

    public async Task<bool> ResetAuthorizations(long authKeyId)
    {
        return ResetAuthorizationsResult;
    }

    public async Task<Data.Auth.SentCode> SendCode(string phoneNumber, int apiId, string apiHash, Data.Auth.CodeSettings settings)
    {
        return new Data.Auth.SentCode()
        {
            CodeType = Data.Auth.SentCodeType.Sms,
            NextType = Data.Auth.SentCodeType.Sms,
            CodeLength = 5,
            Timeout = 60,
            PhoneCodeHash = "acabadef"
        };
    }

    public async Task<Data.Auth.Authorization> SignIn(long authKeyId, string phoneNumber, string phoneCodeHash, string phoneCode)
    {
        if (!_signupComplete)
        {
            return new Data.Auth.Authorization()
            {
                AuthorizationType = AuthorizationType.SignUpRequired
            };
        }
        else
        {
            return new Data.Auth.Authorization()
            {
                AuthorizationType = AuthorizationType.Authorization,
                User = new Data.User()
                {
                    Id = 123,
                    FirstName = _firstName,
                    LastName = _lastName,
                    Phone = phoneNumber,
                    Status = Data.UserStatus.Empty,
                    Self = true,
                    Photo = new Data.UserProfilePhoto()
                    {
                        Empty = true
                    }
                }
            };
        }
    }
    
    public async Task<Data.Auth.Authorization> SignUp(long authKeyId, string phoneNumber, string phoneCodeHash, string firstName, string lastName)
    {
        if(phoneCodeHash!= "acabadef")
        {
            return new Data.Auth.Authorization()
            {
                AuthorizationType = AuthorizationType.SignUpRequired
            };
        }
        _firstName = firstName;
        _lastName = lastName;
        _signupComplete = true;
        return new Data.Auth.Authorization()
        {
            AuthorizationType = AuthorizationType.Authorization,
            User = new Data.User()
            {
                Id = 123,
                FirstName = firstName,
                LastName = lastName,
                Phone = phoneNumber,
                Status = Data.UserStatus.Empty,
                Self = true,
                Photo = new Data.UserProfilePhoto()
                {
                    Empty = true
                }
            }
        };
    }
}
class FakeTime : IMTProtoTime
{
    public long FiveMinutesAgo => long.MinValue;

    public long ThirtySecondsLater => long.MaxValue;
    private Queue<long> unixTimes = new Queue<long>();
    public FakeTime()
    {
        unixTimes.Enqueue(1649323587);
        unixTimes.Enqueue(1649323588);
        unixTimes.Enqueue(1649323588);
        unixTimes.Enqueue(1649323588);
    }
    public long GetUnixTimeInSeconds()
    {
        return unixTimes.Dequeue();
    }
}
class FakeRandom : IRandomGenerator
{
    private int[] generatedPrimes;
    public FakeRandom()
    {
        int rangeEnd = RandomNumberGenerator.GetInt32(int.MaxValue / 4 * 3, int.MaxValue);
        generatedPrimes = RandomGenerator.SieveOfEratosthenesSegmented(rangeEnd - 5000000, rangeEnd);
    }
    public void Fill(Span<byte> data)
    {
        throw new NotImplementedException();
    }

    public int GetNext(int fromInclusive, int toExclusive)
    {
        return 381;
    }

    public byte[] GetRandomBytes(int count)
    {
        if (count == 16)
        {
            return new byte[]
            {
                178, 121,62,117,215,188,141,152,36,193,57,227,183,151,131,37
            };
        }
        return File.ReadAllBytes("testdata/randomBytes_0");
    }

    public BigInteger GetRandomInteger(BigInteger min, BigInteger max)
    {
        RandomNumberGenerator gen = RandomNumberGenerator.Create();
        return RandomInRange(gen, min, max);
    }

    // Implementation was taken from
    // https://stackoverflow.com/a/48855115/2015348
    private static BigInteger RandomInRange(RandomNumberGenerator rng, BigInteger min, BigInteger max)
    {
        if (min > max)
        {
            var buff = min;
            min = max;
            max = buff;
        }

        // offset to set min = 0
        BigInteger offset = -min;
        min = 0;
        max += offset;

        var value = randomInRangeFromZeroToPositive(rng, max) - offset;
        return value;
    }

    private static BigInteger randomInRangeFromZeroToPositive(RandomNumberGenerator rng, BigInteger max)
    {
        BigInteger value;
        var bytes = max.ToByteArray();

        // count how many bits of the most significant byte are 0
        // NOTE: sign bit is always 0 because `max` must always be positive
        byte zeroBitsMask = 0b00000000;

        var mostSignificantByte = bytes[bytes.Length - 1];

        // we try to set to 0 as many bits as there are in the most significant byte, starting from the left (most significant bits first)
        // NOTE: `i` starts from 7 because the sign bit is always 0
        for (var i = 7; i >= 0; i--)
        {
            // we keep iterating until we find the most significant non-0 bit
            if ((mostSignificantByte & (0b1 << i)) != 0)
            {
                var zeroBits = 7 - i;
                zeroBitsMask = (byte)(0b11111111 >> zeroBits);
                break;
            }
        }

        do
        {
            rng.GetBytes(bytes);

            // set most significant bits to 0 (because `value > max` if any of these bits is 1)
            bytes[bytes.Length - 1] &= zeroBitsMask;

            value = new BigInteger(bytes);

            // `value > max` 50% of the times, in which case the fastest way to keep the distribution uniform is to try again
        } while (value > max);

        return value;
    }

    public int GetRandomNumber(int toExclusive)
    {
        return RandomNumberGenerator.GetInt32(toExclusive);
    }

    public int GetRandomNumber(int fromInclusive, int toExclusive)
    {
        return RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);
    }

    public int GetRandomPrime()
    {
        int rnd = RandomNumberGenerator.GetInt32(generatedPrimes.Length);
        return generatedPrimes[rnd];
    }

    public long NextLong()
    {
        return 0;
    }
}
class FakeRedis : IDistributedStore
{
    public FakeRedis()
    {
        byte[] key = File.ReadAllBytes("testdata/authKey_1508830554984586608");
        authKeys.Add(1508830554984586608, key);
        key = File.ReadAllBytes("testdata/authKey_-12783902225236342");
        authKeys.Add(-12783902225236342, key);
    }
    Dictionary<long, byte[]> authKeys = new Dictionary<long, byte[]>();
    Dictionary<long, byte[]> sessions = new Dictionary<long, byte[]>();
    public async Task<byte[]> GetAuthKeyAsync(long authKeyId)
    {
        if (!authKeys.ContainsKey(authKeyId))
        {
            return null;
        }
        return authKeys[authKeyId];
    }

    public async Task<byte[]> GetSessionAsync(long sessionId)
    {
        if (!sessions.ContainsKey(sessionId))
        {
            return null;
        }
        return sessions[sessionId];
    }

    public async Task<bool> PutAuthKeyAsync(long authKeyId, byte[] authKey)
    {
        authKeys.Add(authKeyId, authKey);
        return true;
    }

    public async Task<bool> PutSessionAsync(long sessionId, byte[] sessionData)
    {
        sessions.Add(sessionId, sessionData);
        return true;
    }

    public async Task<bool> RemoveSessionAsync(long sessionId)
    {
        sessions.Remove(sessionId);
        return false;
    }

    public Task<byte[]> GetPhoneCodeAsync(string phoneNumber, string phoneCodeHash)
    {
        throw new NotImplementedException();
    }

    public Task<bool> PutPhoneCodeAsync(string phoneNumber, string phoneCodeHash, string phoneCode, TimeSpan expiresIn)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> GetAuthKeySessionAsync(byte[] nonce)
    {
        throw new NotImplementedException();
    }

    public Task<bool> PutAuthKeySessionAsync(byte[] nonce, byte[] sessionData)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> PutServerSaltAsync(long authKeyId, long serverSalt, long validSince, TimeSpan expiresIn)
    {
        throw new NotImplementedException();
    }

    public Task<long> GetServerSaltValidityAsync(long authKeyId, long serverSalt)
    {
        throw new NotImplementedException();
    }

    Task<string> IDistributedStore.GetPhoneCodeAsync(string phoneNumber, string phoneCodeHash)
    {
        throw new NotImplementedException();
    }

    public IAtomicCounter GetCounter(string name)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeletePhoneCodeAsync(string phoneNumber, string phoneCodeHash)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAuthKeyAsync(long authKeyId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemoveAuthKeySessionAsync(byte[] nonce)
    {
        throw new NotImplementedException();
    }

    public Task<bool> PutTempAuthKeyAsync(long tempAuthKeyId, byte[] tempAuthKey, TimeSpan expiresIn)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]?> GetTempAuthKeyAsync(long tempAuthKeyId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> PutBoundAuthKeyAsync(long tempAuthKeyId, long authKeyId, TimeSpan expiresIn)
    {
        throw new NotImplementedException();
    }

    public Task<long?> GetBoundAuthKeyAsync(long tempAuthKeyId)
    {
        throw new NotImplementedException();
    }
}
class FakeCassandra : IPersistentStore
{
    Dictionary<long, byte[]> authKeys = new Dictionary<long, byte[]>();

    public Task<bool> DeleteAuthKeyAsync(long authKeyId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAuthorizationAsync(long authKeyId)
    {
        throw new NotImplementedException();
    }

    public async Task<byte[]?> GetAuthKeyAsync(long authKeyId)
    {
        if (!authKeys.ContainsKey(authKeyId))
        {
            return null;
        }
        return authKeys[authKeyId];
    }

    public Task<AuthInfo?> GetAuthorizationAsync(long authKeyId)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<AuthInfo>> GetAuthorizationsAsync(string phone)
    {
        throw new NotImplementedException();
    }

    public Task<ExportedAuthInfo?> GetExportedAuthorizationAsync(long user_id, long auth_key_id)
    {
        throw new NotImplementedException();
    }

    public Task<ICollection<ServerSalt>> GetServerSaltsAsync(long authKeyId, int count)
    {
        throw new NotImplementedException();
    }

    public Task<Data.User?> GetUserAsync(long userId)
    {
        throw new NotImplementedException();
    }

    public Task<Data.User?> GetUserAsync(string phone)
    {
        throw new NotImplementedException();
    }

    public Task<Data.User?> GetUserByUsernameAsync(string username)
    {
        throw new NotImplementedException();
    }

    public async Task SaveAuthKeyAsync(long authKeyId, byte[] authKey)
    {
        authKeys.Add(authKeyId, authKey);
    }

    public Task SaveAuthorizationAsync(AuthInfo details)
    {
        throw new NotImplementedException();
    }

    public Task SaveExportedAuthorizationAsync(AuthInfo info, int previousDc, int nextDc, byte[] data)
    {
        throw new NotImplementedException();
    }

    public Task SaveServerSaltAsync(long authKeyId, long serverSalt, long validSince, int TTL)
    {
        throw new NotImplementedException();
    }

    public Task<bool> SaveUserAsync(Data.User user)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateUserAsync(Data.User user)
    {
        throw new NotImplementedException();
    }
}
class FakeDuplexPipe : IDuplexPipe
{
    public FakeDuplexPipe(PipeReader reader, PipeWriter writer)
    {
        Input = reader;
        Output = writer;
    }

    public PipeReader Input { get; }

    public PipeWriter Output { get; }
}
class FakeTransportConnection : ITransportConnection
{
    public IDuplexPipe Transport { get; set; }
    public IDuplexPipe Application { get; set; }
    public Pipe Input { get; set; }
    public Pipe Output { get; set; }
    private string[] _file;

    public FakeTransportConnection(string file = "testdata/obfuscatedIntermediateSession.bin")
    {
        _file = new string[1];
        _file[0] = file;
        Input = new Pipe();
        Output = new Pipe();
        Transport = new FakeDuplexPipe(Input.Reader, Output.Writer);
        Application = new FakeDuplexPipe(Output.Reader, Input.Writer);
    }
    public FakeTransportConnection(params string[] file)
    {
        _file = file;
        Input = new Pipe();
        Output = new Pipe();
        Transport = new FakeDuplexPipe(Input.Reader, Output.Writer);
        Application = new FakeDuplexPipe(Output.Reader, Input.Writer);
    }

    public async void Start()
    {
        foreach (var f in _file)
        {
            byte[] data = File.ReadAllBytes(f);
            await Input.Writer.WriteAsync(data);
        }
    }

    public async Task Receive(string file)
    {
        byte[] data = File.ReadAllBytes(file);
        await Input.Writer.WriteAsync(data);
    }

    public void Abort(Exception abortReason)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
class FakeSessionManager : ISessionManager
{
    public Guid NodeId => Guid.NewGuid();

    private Dictionary<Int128, byte[]> _authKeySessionStates = new();
    private Dictionary<Int128, MTProtoSession> _authKeySessions = new();

    public async Task<bool> AddAuthSessionAsync(byte[] nonce, AuthSessionState state, MTProtoSession session)
    {
        var stateBytes = MessagePackSerializer.Serialize(state);
        _authKeySessions.Add((Int128)nonce, session);
        _authKeySessionStates.Add((Int128)nonce, stateBytes);
        return true;
    }

    public async Task<bool> AddSessionAsync(SessionState state, MTProtoSession session)
    {
        return true;
    }

    public async Task<AuthSessionState?> GetAuthSessionStateAsync(byte[] nonce)
    {
        var rawSession = _authKeySessionStates[(Int128)nonce];
        if (rawSession != null)
        {
            var state = MessagePackSerializer.Deserialize<AuthSessionState>(rawSession);

            return state;
        }
        return null;
    }

    public async Task<SessionState?> GetSessionStateAsync(long sessionId)
    {
        var data = File.ReadAllBytes("testdata/sessionState");
        return MessagePackSerializer.Deserialize<SessionState>(data);
    }

    public bool LocalAuthSessionExists(byte[] nonce)
    {
        throw new NotImplementedException();
    }

    public bool LocalSessionExists(long sessionId)
    {
        throw new NotImplementedException();
    }

    public bool RemoveSession(long sessionId)
    {
        throw new NotImplementedException();
    }

    public bool TryGetLocalAuthSession(byte[] nonce, out MTProtoSession session)
    {
        throw new NotImplementedException();
    }

    public bool TryGetLocalSession(long sessionId, out MTProtoSession session)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateAuthSessionAsync(byte[] nonce, AuthSessionState state)
    {
        _authKeySessionStates.Remove((Int128)nonce);
        _authKeySessionStates.Add((Int128)nonce, MessagePackSerializer.Serialize(state));
        return true;
    }

    public bool RemoveAuthSession(byte[] nonce)
    {
        throw new NotImplementedException();
    }
}
class FakeLogger : ILogger
{
    public void Debug(string message)
    {

    }

    public void Debug(Exception exception, string message)
    {

    }

    public void Error(string message)
    {

    }

    public void Error(Exception exception, string message)
    {

    }

    public void Fatal(string message)
    {

    }

    public void Fatal(Exception exception, string message)
    {

    }

    public void Information(string message)
    {

    }

    public void Information(Exception exception, string message)
    {

    }

    public void Verbose(string message)
    {

    }

    public void Verbose(Exception exception, string message)
    {

    }

    public void Warning(string message)
    {

    }

    public void Warning(Exception exception, string message)
    {

    }
}
class FakeDistributedPipe : IDistributedPipe
{
    ConcurrentQueue<byte[]> _channel = new();
    public async ValueTask<byte[]> ReadAsync(CancellationToken cancellationToken = default)
    {
        _channel.TryDequeue(out var result);
        return result;
    }

    public void Subscribe(string channel)
    {

    }

    public Task SubscribeAsync(string channel)
    {
        throw new NotImplementedException();
    }

    public async Task UnSubscribeAsync()
    {

    }

    public async Task WriteAsync(string channel, byte[] message)
    {
        _channel.Enqueue(message);
    }
}
