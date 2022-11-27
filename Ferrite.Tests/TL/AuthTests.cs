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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using Elasticsearch.Net;
using Ferrite.Core;
using Ferrite.Core.Connection;
using Ferrite.Core.Framing;
using Ferrite.Core.RequestChain;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Account;
using Ferrite.Data.Auth;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.currentLayer;
using Ferrite.TL.currentLayer.auth;
using Ferrite.TL.mtproto;
using Ferrite.TL.ObjectMapper;
using Ferrite.Transport;
using Ferrite.Utils;
using MessagePack;
using Moq;
using Xunit;

namespace Ferrite.Tests.TL;

public class AuthTests
{
    [Fact]
    public async Task SignIn_Returns_SignUpRequired()
    {
        var builder = GetBuilder();
        var auth = new Mock<IAuthService>();
        auth.Setup(x => x.SignIn(It.IsAny<long>(), 
            It.IsAny<string>(), It.IsAny<string>(), 
            It.IsAny<string>())).ReturnsAsync(() => new Ferrite.Data.Auth.AuthorizationDTO()
        {
            AuthorizationType = AuthorizationType.SignUpRequired
        });
        builder.RegisterMock(auth);
        var container = builder.Build();
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.SignIn(It.IsAny<long>(), 
            It.IsAny<string>(), It.IsAny<string>(), 
            It.IsAny<string>())).ReturnsAsync((long authKeyId, string phoneNumber, 
            string phoneCodeHash, string phoneCode) => 
                new Ferrite.Data.Auth.AuthorizationDTO()
            {
                AuthorizationType = AuthorizationType.Authorization,
                User = new Ferrite.Data.UserDTO()
                {
                    Id = 123,
                    FirstName = "a",
                    LastName = "b",
                    Phone = phoneNumber,
                    Status = Ferrite.Data.UserStatusDTO.Empty,
                    Self = true,
                    Photo = new Ferrite.Data.UserProfilePhotoDTO()
                    {
                        Empty = true
                    }
                }
            }
        );
        builder.RegisterMock(authStub);
        var container = builder.Build();
        var factory = container.Resolve<TLObjectFactory>();
        var authService = container.Resolve<IAuthService>();
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
        Assert.IsType<Ferrite.TL.currentLayer.auth.AuthorizationImpl>(rslt.Result);
        var auth = (Ferrite.TL.currentLayer.auth.AuthorizationImpl)rslt.Result;
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.SignUp(It.IsAny<long>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(() =>
            new Ferrite.Data.Auth.AuthorizationDTO()
            {
                AuthorizationType = AuthorizationType.SignUpRequired
            }
        );
        builder.RegisterMock(authStub);
        var container = builder.Build();
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.SignUp(It.IsAny<long>(), It.IsAny<string>(), 
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((long authKeyId, string phoneNumber, string phoneCodeHash, string firstName, string lastName) => 
            new Ferrite.Data.Auth.AuthorizationDTO()
            {
                AuthorizationType = AuthorizationType.Authorization,
                User = new Ferrite.Data.UserDTO()
                {
                    Id = 123,
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = phoneNumber,
                    Status = Ferrite.Data.UserStatusDTO.Empty,
                    Self = true,
                    Photo = new Ferrite.Data.UserProfilePhotoDTO()
                    {
                        Empty = true
                    }
                }
            }
        );
        builder.RegisterMock(authStub);
        var container = builder.Build();
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
        Assert.IsType<Ferrite.TL.currentLayer.auth.AuthorizationImpl>(rslt.Result);
        var auth = (Ferrite.TL.currentLayer.auth.AuthorizationImpl)rslt.Result;
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.LogOut(It.IsAny<long>())).ReturnsAsync((long authKeyId) =>
            {
                if(authKeyId == 0)
                {
                    return null;
                }
                return new Ferrite.Data.Auth.LoggedOutDTO()
                {
                    FutureAuthToken = new byte[] { 1, 2, 3 }
                };
            }
        );
        builder.RegisterMock(authStub);
        var container = builder.Build();
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.LogOut(It.IsAny<long>())).ReturnsAsync((long authKeyId) =>
            {
                if(authKeyId == 0)
                {
                    return null;
                }
                return new Ferrite.Data.Auth.LoggedOutDTO()
                {
                    FutureAuthToken = new byte[] { 1, 2, 3 }
                };
            }
        );
        builder.RegisterMock(authStub);
        var container = builder.Build();
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
        var builder = GetBuilder();
        var auth = new Mock<IAuthService>();
        auth.Setup(x => x.ResetAuthorizations(It.IsAny<long>())).ReturnsAsync(() => true);
        builder.RegisterMock(auth);
        var container = builder.Build();
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
        var builder = GetBuilder();
        var auth = new Mock<IAuthService>();
        auth.Setup(x => x.ResetAuthorizations(It.IsAny<long>())).ReturnsAsync(() => false);
        builder.RegisterMock(auth);
        var container = builder.Build();
        var factory = container.Resolve<TLObjectFactory>();
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.ExportAuthorization(It.IsAny<long>(), It.IsAny<int>())).ReturnsAsync(() =>
            new Ferrite.Data.Auth.ExportedAuthorizationDTO()
            {
                Id = 111,
                Bytes = new byte[] { 1, 2, 3 }
            }
        );
        builder.RegisterMock(authStub);
        var container = builder.Build();
        var factory = container.Resolve<TLObjectFactory>();
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.ImportAuthorization(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<byte[]>()))
            .ReturnsAsync(() =>
                new Ferrite.Data.Auth.AuthorizationDTO()
                {
                    AuthorizationType = AuthorizationType.Authorization,
                    User = new Ferrite.Data.UserDTO()
                    {
                        Id = 123,
                        FirstName = "a",
                        LastName = "b",
                        Phone = "5554443322",
                        Status = Ferrite.Data.UserStatusDTO.Empty,
                        Self = true,
                        Photo = new Ferrite.Data.UserProfilePhotoDTO()
                        {
                            Empty = true
                        }
                    }
                }
        );
        builder.RegisterMock(authStub);
        var container = builder.Build();
        var factory = container.Resolve<TLObjectFactory>();
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
        Assert.IsType<Ferrite.TL.currentLayer.auth.AuthorizationImpl>(rslt.Result);
        var auth = (Ferrite.TL.currentLayer.auth.AuthorizationImpl)rslt.Result;
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.ImportAuthorization(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<byte[]>()))
            .ReturnsAsync(() =>
                new Ferrite.Data.Auth.AuthorizationDTO()
                {
                    AuthorizationType = AuthorizationType.AuthBytesInvalid
                }
            );
        builder.RegisterMock(authStub);
        var container = builder.Build();
        var factory = container.Resolve<TLObjectFactory>();
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
    public async Task CancelCode_Returns_True()
    {
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.CancelCode(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(() =>
                true
            );
        builder.RegisterMock(authStub);
        var container = builder.Build();
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.CancelCode(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(() =>
                false
            );
        builder.RegisterMock(authStub);
        var container = builder.Build();
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.DropTempAuthKeys(It.IsAny<long>(), It.IsAny<ICollection<long>>()))
            .ReturnsAsync(() =>
                true
            );
        builder.RegisterMock(authStub);
        var container = builder.Build();
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.DropTempAuthKeys(It.IsAny<long>(), It.IsAny<ICollection<long>>()))
            .ReturnsAsync(() =>
                false
            );
        builder.RegisterMock(authStub);
        var container = builder.Build();
        var factory = container.Resolve<TLObjectFactory>();
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.ExportLoginToken(It.IsAny<long>(), It.IsAny<long>(), 
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ICollection<long>>()))
            .ReturnsAsync(() =>
                new Ferrite.Data.Auth.LoginTokenDTO()
                {
                    LoginTokenType = LoginTokenType.Token,
                    Expires = 30,
                    Token = new byte[] { 1, 2, 3, 4 }
                }
            );
        builder.RegisterMock(authStub);
        var container = builder.Build();
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
    /* these will not be implemented yet
     [Fact]
    public async Task ImportLoginToken_Returns_LoginToken()
    {
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.ExportLoginToken(It.IsAny<long>(), It.IsAny<long>(), 
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ICollection<long>>()))
            .ReturnsAsync(() =>
                new Data.Auth.LoginToken()
                {
                    LoginTokenType = LoginTokenType.Token,
                    Expires = 30,
                    Token = new byte[] { 1, 2, 3, 4 }
                }
            );
        builder.RegisterMock(authStub);
        var container = builder.Build();
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.ImportLoginToken(It.IsAny<byte[]>()))
            .ReturnsAsync(() =>
                new Data.Auth.LoginToken()
                {
                    LoginTokenType = LoginTokenType.TokenMigrateTo,
                    DcId = 1,
                    Token = new byte[] { 1, 2, 3, 4 }
                }
            );
        builder.RegisterMock(authStub);
        var container = builder.Build();
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.ImportLoginToken(It.IsAny<byte[]>()))
            .ReturnsAsync(() =>
                new Data.Auth.LoginToken()
                {
                    LoginTokenType = LoginTokenType.TokenSuccess,
                    Authorization = new Data.Auth.Authorization()
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
                    }
                }
            );
        builder.RegisterMock(authStub);
        var container = builder.Build();
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
        Assert.IsType<Ferrite.TL.currentLayer.auth.AuthorizationImpl>(token.Authorization);
        var auth = (Ferrite.TL.currentLayer.auth.AuthorizationImpl)token.Authorization;
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.AcceptLoginToken(It.IsAny<long>(), It.IsAny<byte[]>()))
            .ReturnsAsync(() =>
                new AppInfo()
                {
                    ApiId = 1,
                    AppVersion = "1.1",
                    AuthKeyId = 444,
                    SystemVersion = "1.0",
                    DeviceModel = "1.2",
                    IP = "127.0.0.1",
                    LangCode = "tr",
                    LangPack = "Android",
                    SystemLangCode = "tr"
                }
            );
        builder.RegisterMock(authStub);
        var container = builder.Build();
        var factory = container.Resolve<TLObjectFactory>();
        var rpc = factory.Resolve<AcceptLoginToken>();
        rpc.Token = new byte[] { 1, 2, 3 };
        var result = await rpc.ExecuteAsync(new TLExecutionContext(new Dictionary<string, object>())
        {
            MessageId = 1223,
            SessionId = 123
        });
        Assert.IsType<RpcResult>(result);
        var rslt = (RpcResult)result;
        Assert.Equal(1223, rslt.ReqMsgId);
        Assert.IsType<Ferrite.TL.currentLayer.AuthorizationImpl>(rslt.Result);
        var auth = (Ferrite.TL.currentLayer.AuthorizationImpl)rslt.Result;
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.CheckRecoveryPassword(It.IsAny<string>()))
            .ReturnsAsync(() =>
                true
            );
        builder.RegisterMock(authStub);
        var container = builder.Build();
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
        var builder = GetBuilder();
        var authStub = new Mock<IAuthService>();
        authStub.Setup(x => x.CheckRecoveryPassword(It.IsAny<string>()))
            .ReturnsAsync(() =>
                false
            );
        builder.RegisterMock(authStub);
        var container = builder.Build();
        var factory = container.Resolve<TLObjectFactory>();
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
*/
    private ContainerBuilder GetBuilder()
    {
        ConcurrentQueue<byte[]> _channel = new();
        var pipe = new Mock<IMessagePipe>();
        pipe.Setup(x => x.WriteMessageAsync(It.IsAny<string>(), It.IsAny<byte[]>())).ReturnsAsync((string a, byte[] b) =>
        {
            _channel.Enqueue(b);
            return true;
        });
        pipe.Setup(x => x.ReadMessageAsync(default)).Returns(() =>
        {
            _channel.TryDequeue(out var result);
            return ValueTask.FromResult(result!);
        });
        var logger = new Mock<ILogger>();
        Dictionary<long, byte[]> authKeys = new Dictionary<long, byte[]>();
        Dictionary<long, byte[]> sessions = new Dictionary<long, byte[]>();
        byte[] key = File.ReadAllBytes("testdata/authKey_1508830554984586608");
        authKeys.Add(1508830554984586608, key);
        key = File.ReadAllBytes("testdata/authKey_-12783902225236342");
        authKeys.Add(-12783902225236342, key);
        var proto = new Mock<IMTProtoService>();
        proto.Setup(x => x.PutAuthKeyAsync(It.IsAny<long>(), It.IsAny<byte[]>())).ReturnsAsync((long a, byte[] b) =>
        {
            authKeys.Add(a, b);
            return true;
        });
        proto.Setup(x => x.GetAuthKeyAsync(It.IsAny<long>())).ReturnsAsync((long a) =>
        {
            if (!authKeys.ContainsKey(a))
            {
                return new byte[0];
            }
            return authKeys[a];
        });
        Dictionary<long, byte[]> authKeys2 = new Dictionary<long, byte[]>();
        Queue<long> unixTimes = new Queue<long>();
        var time = new Mock<IMTProtoTime>();
        unixTimes.Enqueue(1649323587);
        unixTimes.Enqueue(1649323588);
        unixTimes.Enqueue(1649323588);
        unixTimes.Enqueue(1649323588);
        time.SetupGet(x => x.FiveMinutesAgo).Returns(long.MinValue);
        time.SetupGet(x => x.ThirtySecondsLater).Returns(long.MaxValue);
        time.Setup(x => x.GetUnixTimeInSeconds()).Returns(() => unixTimes.Dequeue());
        int rangeEnd = RandomNumberGenerator.GetInt32(int.MaxValue / 4 * 3, int.MaxValue);
        var generatedPrimes = RandomGenerator.SieveOfEratosthenesSegmented(rangeEnd - 5000000, rangeEnd);
        var random = new Mock<IRandomGenerator>();
        RandomGenerator rnd = new RandomGenerator();
        random.Setup(x => x.GetNext(It.IsAny<int>(),It.IsAny<int>()))
            .Returns(()=> 381);
        random.Setup(x => x.GetRandomBytes(It.IsAny<int>()))
            .Returns((int count) =>
            {
                if(count == 16)
                {
                    return new byte[]
                    {
                        178, 121,62,117,215,188,141,152,36,193,57,227,183,151,131,37
                    };
                }
                return File.ReadAllBytes("testdata/randomBytes_0");
            });
        random.Setup(x => x.GetRandomInteger(It.IsAny<BigInteger>(),It.IsAny<BigInteger>()))
            .Returns((int a, int b)=> rnd.GetRandomInteger(a,b));
        random.Setup(x => x.GetRandomNumber(It.IsAny<int>()))
            .Returns((int a)=> rnd.GetRandomNumber(a));
        random.Setup(x => x.GetRandomNumber(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((int a, int b)=> rnd.GetRandomNumber(a, b));
        random.Setup(x => x.GetRandomPrime())
            .Returns(()=> rnd.GetRandomPrime());
        Dictionary<Ferrite.TL.Int128, byte[]> _authKeySessionStates = new(); 
        Dictionary<Ferrite.TL.Int128, ActiveSession> _authKeySessions = new();
        var sessionManager = new Mock<ISessionService>();
        sessionManager.SetupGet(x => x.NodeId).Returns(Guid.NewGuid());
        sessionManager.Setup(x => x.AddAuthSessionAsync(It.IsAny<byte[]>(),
            It.IsAny<AuthSessionState>(), It.IsAny<ActiveSession>())).ReturnsAsync(
            (byte[] nonce, AuthSessionState state, ActiveSession session) =>
        {
            var stateBytes = MessagePackSerializer.Serialize(state);
            _authKeySessions.Add((Ferrite.TL.Int128)nonce, session);
            _authKeySessionStates.Add((Ferrite.TL.Int128)nonce, stateBytes);
            return true;
        });
        sessionManager.Setup(x => x.AddSessionAsync(It.IsAny<long>(), It.IsAny<long>(),
            It.IsAny<ActiveSession>())).ReturnsAsync(() => true);
        sessionManager.Setup(x => x.GetAuthSessionStateAsync(It.IsAny<byte[]>())).ReturnsAsync((byte[] nonce) =>
        {
            var rawSession = _authKeySessionStates[(Ferrite.TL.Int128)nonce];
            if (rawSession != null)
            {
                var state = MessagePackSerializer.Deserialize<AuthSessionState>(rawSession);

                return state;
            }
            return null;
        });
        sessionManager.Setup(x => x.GetSessionStateAsync(It.IsAny<long>())).ReturnsAsync((long sessionId) =>
        {
            var data = File.ReadAllBytes("testdata/sessionState");
            return MessagePackSerializer.Deserialize<RemoteSession>(data);
        });
        sessionManager.Setup(x => x.UpdateAuthSessionAsync(It.IsAny<byte[]>(), It.IsAny<AuthSessionState>()))
            .ReturnsAsync(
                (byte[] nonce, AuthSessionState state) =>
                {
                    _authKeySessionStates.Remove((Ferrite.TL.Int128)nonce);
                    _authKeySessionStates.Add((Ferrite.TL.Int128)nonce, MessagePackSerializer.Serialize(state));
                    return true;
                });
        
        var tl = Assembly.Load("Ferrite.TL");
        var builder = new ContainerBuilder();
        builder.RegisterMock(time);
        builder.RegisterMock(random);
        builder.RegisterType<KeyProvider>().As<IKeyProvider>();
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
            .Where(t => t.Namespace != null && t.Namespace.StartsWith("Ferrite.TL.currentLayer"))
            .AsSelf();
        builder.Register(_ => new Ferrite.TL.Int128());
        builder.Register(_ => new Int256());
        builder.RegisterType<MTProtoConnection>();
        builder.RegisterType<TLObjectFactory>().As<ITLObjectFactory>();
        builder.RegisterType<DefaultMapper>().As<IMapperContext>();
        builder.RegisterType<MTProtoTransportDetector>().As<ITransportDetector>();
        builder.RegisterType<SocketConnectionListener>().As<IConnectionListener>();
        builder.RegisterMock(proto);
        builder.RegisterMock(logger);
        builder.RegisterMock(sessionManager);
        builder.RegisterType<AuthKeyProcessor>();
        builder.RegisterType<MsgContainerProcessor>();
        builder.RegisterType<ServiceMessagesProcessor>();
        builder.RegisterType<AuthorizationProcessor>();
        builder.RegisterType<MTProtoRequestProcessor>();
        builder.RegisterType<RequestPipeline>().As<IRequestPipeline>().SingleInstance();
        builder.RegisterMock(pipe);

        return builder;
    }
}

