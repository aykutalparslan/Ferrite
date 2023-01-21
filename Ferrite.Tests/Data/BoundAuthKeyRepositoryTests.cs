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
using Moq;
using Xunit;

namespace Ferrite.Tests.Data;

public class BoundAuthKeyRepositoryTests
{
    [Fact]
    public void Puts_BoundAuthKey()
    {
        var authKeyIdBytes = RandomNumberGenerator.GetBytes(8);
        var authKeyId = BitConverter.ToInt64(authKeyIdBytes);
        var tempAuthKeyIdBytes = RandomNumberGenerator.GetBytes(8);
        var tempAuthKeyId = BitConverter.ToInt64(tempAuthKeyIdBytes);
        TimeSpan expiresIn = new TimeSpan(1, 0, 0);
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IVolatileKVStore>();
        store.Setup(x => x.Put(authKeyIdBytes, expiresIn, tempAuthKeyId)).Verifiable();

        var store2 = mock.Mock<IVolatileKVStore>();
        store2.Setup(x => x.Put(tempAuthKeyIdBytes, expiresIn, authKeyId)).Verifiable();
        
        var store3 = mock.Mock<IVolatileKVStore>();
        store3.Setup(x => x.ListAdd(It.IsAny<long>(), tempAuthKeyIdBytes, expiresIn, authKeyId)).Verifiable();

        var repo = new BoundAuthKeyRepository(store.Object, store2.Object, store3.Object);
        repo.PutBoundAuthKey(tempAuthKeyId, authKeyId, expiresIn);
        store.VerifyAll();
        store2.VerifyAll();
        store3.VerifyAll();
    }
    
    [Fact]
    public void Gets_BoundAuthKey()
    {
        var authKeyIdBytes = RandomNumberGenerator.GetBytes(8);
        var authKeyId = BitConverter.ToInt64(authKeyIdBytes);
        var tempAuthKeyIdBytes = RandomNumberGenerator.GetBytes(8);
        var tempAuthKeyId = BitConverter.ToInt64(tempAuthKeyIdBytes);
        TimeSpan expiresIn = new TimeSpan(1, 0, 0);
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IVolatileKVStore>();
        store.Setup(x => x.Get(tempAuthKeyId)).Returns(authKeyIdBytes);

        var store2 = mock.Mock<IVolatileKVStore>();
        store2.Setup(x => x.Get(authKeyId)).Returns(tempAuthKeyIdBytes);
        
        var store3 = mock.Mock<IVolatileKVStore>();

        var repo = new BoundAuthKeyRepository(store.Object, store2.Object, store3.Object);
        repo.PutBoundAuthKey(tempAuthKeyId, authKeyId, expiresIn);
        var key = repo.GetBoundAuthKey(tempAuthKeyId);
        Assert.Equal(authKeyId, key);
    }
    
    [Fact]
    public async Task Gets_BoundAuthKeyAsync()
    {
        var authKeyIdBytes = RandomNumberGenerator.GetBytes(8);
        var authKeyId = BitConverter.ToInt64(authKeyIdBytes);
        var tempAuthKeyIdBytes = RandomNumberGenerator.GetBytes(8);
        var tempAuthKeyId = BitConverter.ToInt64(tempAuthKeyIdBytes);
        TimeSpan expiresIn = new TimeSpan(1, 0, 0);
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IVolatileKVStore>();
        store.Setup(x => x.GetAsync(tempAuthKeyId)).ReturnsAsync(authKeyIdBytes);

        var store2 = mock.Mock<IVolatileKVStore>();
        store2.Setup(x => x.GetAsync(authKeyId)).ReturnsAsync(tempAuthKeyIdBytes);
        
        var store3 = mock.Mock<IVolatileKVStore>();

        var repo = new BoundAuthKeyRepository(store.Object, store2.Object, store3.Object);
        repo.PutBoundAuthKey(tempAuthKeyId, authKeyId, expiresIn);
        var key = await repo.GetBoundAuthKeyAsync(tempAuthKeyId);
        Assert.Equal(authKeyId, key);
    }
    
    [Fact]
    public void Gets_TempAuthKeys()
    {
        List<byte[]> tempAuthKeys = new();
        for (int i = 0; i < 3; i++)
        {
            var tempAuthKeyIdBytes = RandomNumberGenerator.GetBytes(8);
            tempAuthKeys.Add(tempAuthKeyIdBytes);
        }
        var authKeyIdBytes = RandomNumberGenerator.GetBytes(8);
        var authKeyId = BitConverter.ToInt64(authKeyIdBytes);
        TimeSpan expiresIn = new TimeSpan(1, 0, 0);
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IVolatileKVStore>();
        var store2 = mock.Mock<IVolatileKVStore>();
        var store3 = mock.Mock<IVolatileKVStore>();
        store3.Setup(x=>x.ListDeleteByScore(It.IsAny<long>(), authKeyIdBytes)).Verifiable();
        store3.Setup(x => x.ListGet(authKeyId)).Returns(tempAuthKeys);
        var repo = new BoundAuthKeyRepository(store.Object, store2.Object, store3.Object);
        var keys = repo.GetTempAuthKeys(authKeyId);
        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(BitConverter.ToInt64(tempAuthKeys[i]), keys[i]);
        }
    }
    
    [Fact]
    public void PutsAndGets_BoundAuthKey()
    {
        var repo = new BoundAuthKeyRepository(new InMemoryStore(), new InMemoryStore(), new InMemoryStore());
        var authKeyIdBytes = RandomNumberGenerator.GetBytes(8);
        var authKeyId = BitConverter.ToInt64(authKeyIdBytes);
        var tempAuthKeyIdBytes = RandomNumberGenerator.GetBytes(8);
        var tempAuthKeyId = BitConverter.ToInt64(tempAuthKeyIdBytes);
        TimeSpan expiresIn = new TimeSpan(1, 0, 0);
        repo.PutBoundAuthKey(tempAuthKeyId, authKeyId, expiresIn);
        var key = repo.GetBoundAuthKey(tempAuthKeyId);
        Assert.NotNull(key);
        Assert.Equal(authKeyId, key);
    }
    
    [Fact]
    public async Task PutsAndGets_BoundAuthKeyAsync()
    {
        var repo = new BoundAuthKeyRepository(new InMemoryStore(), new InMemoryStore(), new InMemoryStore());
        var authKeyIdBytes = RandomNumberGenerator.GetBytes(8);
        var authKeyId = BitConverter.ToInt64(authKeyIdBytes);
        var tempAuthKeyIdBytes = RandomNumberGenerator.GetBytes(8);
        var tempAuthKeyId = BitConverter.ToInt64(tempAuthKeyIdBytes);
        TimeSpan expiresIn = new TimeSpan(1, 0, 0);
        repo.PutBoundAuthKey(tempAuthKeyId, authKeyId, expiresIn);
        var key = await repo.GetBoundAuthKeyAsync(tempAuthKeyId);
        Assert.NotNull(key);
        Assert.Equal(authKeyId, key);
    }
    
    [Fact]
    public void PutsAndGets_TempAuthKeys()
    {
        var repo = new BoundAuthKeyRepository(new InMemoryStore(), new InMemoryStore(), new InMemoryStore());
        var authKeyIdBytes = RandomNumberGenerator.GetBytes(8);
        var authKeyId = BitConverter.ToInt64(authKeyIdBytes);
        TimeSpan expiresIn = new TimeSpan(1, 0, 0);
        List<byte[]> tempAuthKeys = new();
        for (int i = 0; i < 3; i++)
        {
            var tempAuthKeyIdBytes = RandomNumberGenerator.GetBytes(8);
            var tempAuthKeyId = BitConverter.ToInt64(tempAuthKeyIdBytes);
            tempAuthKeys.Add(tempAuthKeyIdBytes);
            repo.PutBoundAuthKey(tempAuthKeyId, authKeyId, expiresIn);
        }
        var keys = repo.GetTempAuthKeys(authKeyId);
        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(BitConverter.ToInt64(tempAuthKeys[i]), keys[i]);
        }
    }
}