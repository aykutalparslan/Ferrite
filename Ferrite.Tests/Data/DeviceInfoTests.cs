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
using Ferrite.TL.slim.layer150.dto;
using Xunit;

namespace Ferrite.Tests.Data;

public class DeviceInfoTests
{
    [Fact]
    public void Puts_DeviceInfo()
    {
        using TLDeviceInfo info = DeviceInfo.Builder()
            .TokenType(2)
            .Token("aaa111"u8)
            .OtherUids(new VectorOfLong())
            .Secret("test123"u8)
            .AppSandbox(true)
            .NoMuted(true)
            .Build();
        var infoBytes = info.AsSpan().ToArray();
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(infoBytes, 
            (long)111, "aaa111")).Verifiable();

        var repo = mock.Create<DeviceInfoRepository>();
        repo.PutDeviceInfo(111, info);
        store.VerifyAll();
    }
    
    [Fact]
    public void Gets_DeviceInfo()
    {
        using TLDeviceInfo info = DeviceInfo.Builder()
            .TokenType(2)
            .Token("aaa111"u8)
            .OtherUids(new VectorOfLong())
            .Secret("test123"u8)
            .AppSandbox(true)
            .NoMuted(true)
            .Build();
        var infoBytes = info.AsSpan().ToArray();
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Get((long)111)).Returns(infoBytes);

        var repo = mock.Create<DeviceInfoRepository>();
        var result = repo.GetDeviceInfo(111);
        Assert.NotNull(result);
        Assert.Equal(infoBytes, result.Value.AsSpan().ToArray());
    }
    
    [Fact]
    public void Deletes_DeviceInfo()
    {
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Delete((long)111, "aaa")).Verifiable();

        var repo = mock.Create<DeviceInfoRepository>();
        repo.DeleteDeviceInfo(111, "aaa", new List<long>());
        store.VerifyAll();
    }
}