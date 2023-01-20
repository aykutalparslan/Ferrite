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
using Moq;
using Xunit;

namespace Ferrite.Tests.Data;

public class AuthKeyRepositoryTests
{
    [Fact]
    public void Puts_AuthKey()
    {
        var authKey = RandomNumberGenerator.GetBytes(192);
        var authKeyId = BitConverter.ToInt64(RandomNumberGenerator.GetBytes(8));
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(authKey,
            authKeyId)).Verifiable();

        var store2 = mock.Mock<IVolatileKVStore>();
        store2.Setup(x => x.Put(authKey, 
            null, authKeyId)).Verifiable();

        var repo = mock.Create<AuthKeyRepository>();
        repo.PutAuthKey(authKeyId, authKey);
        store.VerifyAll();
        store2.VerifyAll();
    }
    
    [Fact]
    public void Deletes_AuthKey()
    {
        var authKey = RandomNumberGenerator.GetBytes(192);
        var authKeyId = BitConverter.ToInt64(RandomNumberGenerator.GetBytes(8));
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Delete(authKeyId)).Verifiable();

        var store2 = mock.Mock<IVolatileKVStore>();
        store2.Setup(x => x.Delete(authKeyId)).Verifiable();

        var repo = mock.Create<AuthKeyRepository>();
        repo.DeleteAuthKey(authKeyId);
        store.VerifyAll();
        store2.VerifyAll();
    }
    
    [Fact]
    public void Gets_AuthKey_FromTempStore()
    {
        var authKey = RandomNumberGenerator.GetBytes(192);
        var authKeyId = BitConverter.ToInt64(RandomNumberGenerator.GetBytes(8));
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IVolatileKVStore>();
        store.Setup(x => x.Get(authKeyId)).Returns(authKey);

        var repo = mock.Create<AuthKeyRepository>();
        var key = repo.GetAuthKey(authKeyId);
        Assert.Equal(authKey, key);
        store.VerifyAll();
    }
    
    [Fact]
    public void Gets_AuthKey_FromStore()
    {
        var authKey = RandomNumberGenerator.GetBytes(192);
        var authKeyId = BitConverter.ToInt64(RandomNumberGenerator.GetBytes(8));
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IVolatileKVStore>();
        store.Setup(x => x.Get(authKeyId)).Returns(default(byte[]));
        var store2 = mock.Mock<IKVStore>();
        store2.Setup(x => x.Get(authKeyId)).Returns(authKey);

        var repo = mock.Create<AuthKeyRepository>();
        var key = repo.GetAuthKey(authKeyId);
        Assert.Equal(authKey, key);
        store.VerifyAll();
        store2.VerifyAll();
    }
    
    [Fact]
    public async Task Gets_AuthKey_FromTempStoreAsync()
    {
        var authKey = RandomNumberGenerator.GetBytes(192);
        var authKeyId = BitConverter.ToInt64(RandomNumberGenerator.GetBytes(8));
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IVolatileKVStore>();
        store.Setup(x => x.GetAsync(authKeyId)).ReturnsAsync(authKey);

        var repo = mock.Create<AuthKeyRepository>();
        var key = await repo.GetAuthKeyAsync(authKeyId);
        Assert.Equal(authKey, key);
        store.VerifyAll();
    }
    
    [Fact]
    public async Task Gets_AuthKey_FromStoreAsync()
    {
        var authKey = RandomNumberGenerator.GetBytes(192);
        var authKeyId = BitConverter.ToInt64(RandomNumberGenerator.GetBytes(8));
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IVolatileKVStore>();
        store.Setup(x => x.GetAsync(authKeyId)).ReturnsAsync(default(byte[]));
        var store2 = mock.Mock<IKVStore>();
        store2.Setup(x => x.GetAsync(authKeyId)).ReturnsAsync(authKey);

        var repo = mock.Create<AuthKeyRepository>();
        var key = await repo.GetAuthKeyAsync(authKeyId);
        Assert.Equal(authKey, key);
        store.VerifyAll();
        store2.VerifyAll();
    }
    
    [Fact]
    public void Get_Returns_Null()
    {
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IVolatileKVStore>();
        store.Setup(x => x.Get(It.IsAny<long>())).Returns(default(byte[]));
        var store2 = mock.Mock<IKVStore>();
        store2.Setup(x => x.Get(It.IsAny<long>())).Returns(default(byte[]));

        var repo = mock.Create<AuthKeyRepository>();
        var key = repo.GetAuthKey(123);
        Assert.Null( key);
        store.VerifyAll();
    }
    
    [Fact]
    public async Task Get_Returns_NullAsync()
    {
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IVolatileKVStore>();
        store.Setup(x => x.GetAsync(It.IsAny<long>())).ReturnsAsync(default(byte[]));
        var store2 = mock.Mock<IKVStore>();
        store2.Setup(x => x.GetAsync(It.IsAny<long>())).ReturnsAsync(default(byte[]));
        var repo = mock.Create<AuthKeyRepository>();
        var key = await repo.GetAuthKeyAsync(123);
        Assert.Null( key);
        store.VerifyAll();
    }
    
    [Fact]
    public void PutsAndGets_AuthKey()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new AuthKeyRepository(new RocksDBKVStore(ctx), new InMemoryStore());
        var authKey = RandomNumberGenerator.GetBytes(192);
        var authKeyId = BitConverter.ToInt64(RandomNumberGenerator.GetBytes(8));
        repo.PutAuthKey(authKeyId, authKey);

        var key = repo.GetAuthKey(authKeyId);
        Assert.NotNull(key);
        Assert.Equal(authKey, key);
        Util.DeleteDirectory(path);
    }
    [Fact]
    public async Task PutsAndGets_AuthKeyAsync()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new AuthKeyRepository(new RocksDBKVStore(ctx), new InMemoryStore());
        var authKey = RandomNumberGenerator.GetBytes(192);
        var authKeyId = BitConverter.ToInt64(RandomNumberGenerator.GetBytes(8));
        repo.PutAuthKey(authKeyId, authKey);

        var key = await repo.GetAuthKeyAsync(authKeyId);
        Assert.NotNull(key);
        Assert.Equal(authKey, key);
        Util.DeleteDirectory(path);
    }
    
    [Fact]
    public void PutsAndDeletes_AuthKey()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new AuthKeyRepository(new RocksDBKVStore(ctx), new InMemoryStore());
        var authKey = RandomNumberGenerator.GetBytes(192);
        var authKeyId = BitConverter.ToInt64(RandomNumberGenerator.GetBytes(8));
        repo.PutAuthKey(authKeyId, authKey);
        repo.DeleteAuthKey(authKeyId);
        var key = repo.GetAuthKey(authKeyId);
        Assert.Null(key);
        Util.DeleteDirectory(path);
    }
}