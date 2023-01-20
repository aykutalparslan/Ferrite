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
using Xunit;

namespace Ferrite.Tests.Data;

public class AuthSessionRepositoryTests
{
    [Fact]
    public void Puts_AuthKeySession()
    {
        var sessionData = RandomNumberGenerator.GetBytes(128);
        var nonce = RandomNumberGenerator.GetBytes(16);
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IVolatileKVStore>();
        store.Setup(x => x.Put(sessionData, null, nonce)).Verifiable();

        var repo = mock.Create<AuthSessionRepository>();
        repo.PutAuthKeySession(nonce, sessionData);
        store.VerifyAll();
    }
    
    [Fact]
    public void Removes_AuthKeySession()
    {
        var sessionData = RandomNumberGenerator.GetBytes(128);
        var nonce = RandomNumberGenerator.GetBytes(16);
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IVolatileKVStore>();
        store.Setup(x => x.Delete(nonce)).Verifiable();

        var repo = mock.Create<AuthSessionRepository>();
        repo.RemoveAuthKeySession(nonce);
        store.VerifyAll();
    }
    
    [Fact]
    public void Gets_AuthKeySession()
    {
        var sessionData = RandomNumberGenerator.GetBytes(128);
        var nonce = RandomNumberGenerator.GetBytes(16);
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IVolatileKVStore>();
        store.Setup(x => x.Get(nonce)).Returns(sessionData);

        var repo = mock.Create<AuthSessionRepository>();
        var sess = repo.GetAuthKeySession(nonce);
        Assert.Equal(sessionData, sessionData);
        store.VerifyAll();
    }
    
    [Fact]
    public void Get_Returns_Null()
    {
        var sessionData = RandomNumberGenerator.GetBytes(128);
        var nonce = RandomNumberGenerator.GetBytes(16);
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IVolatileKVStore>();
        store.Setup(x => x.Get(nonce)).Returns(default(byte[]?));

        var repo = mock.Create<AuthSessionRepository>();
        var sess = repo.GetAuthKeySession(nonce);
        Assert.Null(sess);
        store.VerifyAll();
    }
    
    [Fact]
    public void PutsAndGets_AuthSession()
    {
        string path = "test-" + Random.Shared.Next();
        var repo = new AuthSessionRepository(new InMemoryStore());
        var sessionData = RandomNumberGenerator.GetBytes(128);
        var nonce = RandomNumberGenerator.GetBytes(16);
        repo.PutAuthKeySession(nonce, sessionData);
        var sess = repo.GetAuthKeySession(nonce);
        Assert.Equal(sessionData, sess);
        Util.DeleteDirectory(path);
    }
    
    [Fact]
    public void PutsAndRemoves_AuthSession()
    {
        string path = "test-" + Random.Shared.Next();
        var repo = new AuthSessionRepository(new InMemoryStore());
        var sessionData = RandomNumberGenerator.GetBytes(128);
        var nonce = RandomNumberGenerator.GetBytes(16);
        repo.PutAuthKeySession(nonce, sessionData);
        repo.RemoveAuthKeySession(nonce);
        var sess = repo.GetAuthKeySession(nonce);
        Assert.Null(sess);
        Util.DeleteDirectory(path);
    }
}