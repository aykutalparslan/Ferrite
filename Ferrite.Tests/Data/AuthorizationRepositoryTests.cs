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

using System.Security.Cryptography;
using Autofac.Extras.Moq;
using Ferrite.Data.Repositories;
using Ferrite.TL.slim;
using Ferrite.TL.slim.dto;
using Xunit;

namespace Ferrite.Tests.Data;

public class AuthorizationRepositoryTests
{
    [Fact]
    public void Puts_Authorization()
    {
        var infoBytes = GenerateInfoBytes(111, "54321"u8);
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(infoBytes, 
            (long)111, "54321")).Verifiable();

        var repo = mock.Create<AuthorizationRepository>();
        repo.PutAuthorization(new TLAuthInfo(infoBytes,0 , infoBytes.Length));
        store.VerifyAll();
    }
    
    [Fact]
    public void PutsAndGets_Authorization()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new AuthorizationRepository(new RocksDBKVStore(ctx), new RocksDBKVStore(ctx));
        var infoBytes = GenerateInfoBytes(111, "54321"u8);
        repo.PutAuthorization(new TLAuthInfo(infoBytes,0 , infoBytes.Length));

        var info = repo.GetAuthorization(111);
        Assert.NotNull(info);
        Assert.Equal(infoBytes, info.Value.AsSpan().ToArray());
        Util.DeleteDirectory(path);
    }
    
    [Fact]
    public async Task PutsAndGets_AuthorizationAsync()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new AuthorizationRepository(new RocksDBKVStore(ctx), new RocksDBKVStore(ctx));
        var infoBytes = GenerateInfoBytes(111, "54321"u8);
        repo.PutAuthorization(new TLAuthInfo(infoBytes,0 , infoBytes.Length));

        var info = await repo.GetAuthorizationAsync(111);
        Assert.NotNull(info);
        Assert.Equal(infoBytes, info.Value.AsSpan().ToArray());
        Util.DeleteDirectory(path);
    }
    
    [Fact]
    public void PutsAndGets_Authorizations()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new AuthorizationRepository(new RocksDBKVStore(ctx), new RocksDBKVStore(ctx));
        var infoBytes = GenerateInfoBytes(111, "54321"u8);
        repo.PutAuthorization(new TLAuthInfo(infoBytes,0 , infoBytes.Length));
        var infoBytes2 = GenerateInfoBytes(222, "54321"u8);
        repo.PutAuthorization(new TLAuthInfo(infoBytes2,0 , infoBytes2.Length));

        var infos = repo.GetAuthorizations("54321");
        Assert.Equal(2, infos.Count);
        Assert.Equal(infoBytes, infos[0].AsSpan().ToArray());
        Assert.Equal(infoBytes2, infos[1].AsSpan().ToArray());
        Util.DeleteDirectory(path);
    }
    
    [Fact]
    public async Task PutsAndGets_AuthorizationsAsync()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new AuthorizationRepository(new RocksDBKVStore(ctx), new RocksDBKVStore(ctx));
        var infoBytes = GenerateInfoBytes(111, "54321"u8);
        repo.PutAuthorization(new TLAuthInfo(infoBytes,0 , infoBytes.Length));
        var infoBytes2 = GenerateInfoBytes(222, "54321"u8);
        repo.PutAuthorization(new TLAuthInfo(infoBytes2,0 , infoBytes2.Length));

        var infos = await repo.GetAuthorizationsAsync("54321");
        Assert.Equal(2, infos.Count);
        Assert.Equal(infoBytes, infos[0].AsSpan().ToArray());
        Assert.Equal(infoBytes2, infos[1].AsSpan().ToArray());
        Util.DeleteDirectory(path);
    }
    
    [Fact]
    public void Deletes_Authorization()
    {
        var infoBytes = GenerateInfoBytes(111, "54321"u8);
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(infoBytes, 
            (long)111, "54321")).Verifiable();
        store.Setup(x => x.Delete((long)111)).Verifiable();

        var repo = mock.Create<AuthorizationRepository>();
        repo.PutAuthorization(new TLAuthInfo(infoBytes,0 , infoBytes.Length));
        repo.DeleteAuthorization(111);
        store.VerifyAll();
    }
    
    [Fact]
    public void Puts_ExportedAuthorization()
    {
        var data = RandomNumberGenerator.GetBytes(16);
        var infoBytes = GenerateExportedInfoBytes(data);
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(infoBytes, 
            (long)123, data)).Verifiable();

        var repo = mock.Create<AuthorizationRepository>();
        repo.PutExportedAuthorization(new TLExportedAuthInfo(infoBytes,0 , infoBytes.Length));
        store.VerifyAll();
    }
    
    [Fact]
    public void PutsAndGets_ExportedAuthorization()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new AuthorizationRepository(new RocksDBKVStore(ctx), new RocksDBKVStore(ctx));
        var data = RandomNumberGenerator.GetBytes(16);
        var infoBytes = GenerateExportedInfoBytes(data);
        repo.PutExportedAuthorization(new TLExportedAuthInfo(infoBytes,0 , infoBytes.Length));

        var info = repo.GetExportedAuthorization(123, data);
        Assert.NotNull(info);
        Assert.Equal(infoBytes, info.Value.AsSpan().ToArray());
        Util.DeleteDirectory(path);
    }
    
    [Fact]
    public async Task PutsAndGets_ExportedAuthorizationAsync()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new AuthorizationRepository(new RocksDBKVStore(ctx), new RocksDBKVStore(ctx));
        var data = RandomNumberGenerator.GetBytes(16);
        var infoBytes = GenerateExportedInfoBytes(data);
        repo.PutExportedAuthorization(new TLExportedAuthInfo(infoBytes,0 , infoBytes.Length));

        var info = await repo.GetExportedAuthorizationAsync(123, data);
        Assert.NotNull(info);
        Assert.Equal(infoBytes, info.Value.AsSpan().ToArray());
        Util.DeleteDirectory(path);
    }
    
    private byte[] GenerateExportedInfoBytes(byte[] data)
    {
        return ExportedAuthInfo.Builder()
            .AuthKeyId(111)
            .UserId(123)
            .Phone("5432"u8)
            .Data(data)
            .NextDcId(2)
            .PreviousDcId(1)
            .Build().TLBytes!.Value.AsSpan().ToArray();
    }
    
    private byte[] GenerateInfoBytes(long authKeyId, ReadOnlySpan<byte> phone)
    {
        return AuthInfo.Builder()
            .AuthKeyId(authKeyId)
            .Phone(phone)
            .UserId(123)
            .ApiLayer(150)
            .LoggedIn(true)
            .FutureAuthToken(RandomNumberGenerator.GetBytes(16))
            .LoggedInAt((int)DateTimeOffset.Now.ToUnixTimeSeconds())
            .Build().TLBytes!.Value.AsSpan().ToArray();
    }
}