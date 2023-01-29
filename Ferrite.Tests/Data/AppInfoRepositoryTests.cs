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

public class AppInfoRepositoryTests
{
    [Fact]
    public void Puts_AppInfo()
    {
        var infoBytes = GenerateInfoBytes();
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(infoBytes, 
            (long)111, (long)222)).Verifiable();

        var repo = mock.Create<AppInfoRepository>();
        repo.PutAppInfo(new TLAppInfo(infoBytes,0 , infoBytes.Length));
        store.VerifyAll();
    }

    private byte[] GenerateInfoBytes()
    {
        return AppInfo.Builder()
            .AuthKeyId(111)
            .Hash(222)
            .ApiId(555)
            .AppVersion("asdf"u8)
            .DeviceModel("dffsdaf"u8)
            .Ip("127.0.0.1"u8)
            .LangCode("tr"u8)
            .LangPack("tr"u8)
            .SystemLangCode("tr"u8)
            .SystemVersion("0.01"u8)
            .Build().TLBytes!.Value.AsSpan().ToArray();
    }

    [Fact]
    public void PutsAndGets_AppInfo()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new AppInfoRepository(new RocksDBKVStore(ctx));
        var infoBytes = GenerateInfoBytes();
        repo.PutAppInfo(new TLAppInfo(infoBytes,0 , infoBytes.Length));

        var info = repo.GetAppInfo(111);
        Assert.NotNull(info);
        Assert.Equal(infoBytes, info.Value.AsSpan().ToArray());
        Util.DeleteDirectory(path);
    }
    
    [Fact]
    public void PutsAppInfoAnd_GetsByAppHash()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new AppInfoRepository(new RocksDBKVStore(ctx));
        var infoBytes = GenerateInfoBytes();
        repo.PutAppInfo(new TLAppInfo(infoBytes,0 , infoBytes.Length));

        var info = repo.GetAppInfoByAppHash(222);
        Assert.NotNull(info);
        Assert.Equal(infoBytes, info.Value.AsSpan().ToArray());
        Util.DeleteDirectory(path);
    }
    
    [Fact]
    public void PutsAppInfoAndGets_AuthKeyIdByAppHash()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new AppInfoRepository(new RocksDBKVStore(ctx));
        var infoBytes = GenerateInfoBytes();
        repo.PutAppInfo(new TLAppInfo(infoBytes,0 , infoBytes.Length));

        var authKeyId = repo.GetAuthKeyIdByAppHash(222);
        Assert.NotNull(authKeyId);
        Assert.Equal(111, authKeyId.Value);
        Util.DeleteDirectory(path);
    }
}