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

using System.Text;
using Ferrite.Data.Repositories;
using Ferrite.TL.slim.baseLayer;
using Xunit;

namespace Ferrite.Tests.Data;

public class UserRepositoryTests
{
    [Fact]
    public void Should_PutAndGet_User()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        UserRepository repo = new UserRepository(new RocksDBKVStore(ctx), 
            new RocksDBKVStore(ctx), new RocksDBKVStore(ctx));
        using var user = User.Builder()
            .Id(123)
            .Username("test"u8)
            .Phone("+111222"u8)
            .FirstName("aaa"u8)
            .LastName("bbb"u8)
            .Build();
        repo.PutUser(user);
        var userBytes =  repo.GetUser(123);
        Assert.Equal(user.TLBytes!.Value.AsSpan().ToArray(), 
            userBytes!.Value.AsSpan().ToArray());
        Util.DeleteDirectory(path);
    }
    [Fact]
    public void Should_GetUserByPhone()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        UserRepository repo = new UserRepository(new RocksDBKVStore(ctx), 
            new RocksDBKVStore(ctx),new RocksDBKVStore(ctx));
        using var user = User.Builder()
            .Id(123)
            .Username("test"u8)
            .Phone("+111222"u8)
            .FirstName("aaa"u8)
            .LastName("bbb"u8)
            .Build();
        repo.PutUser(user);
        var userBytes = repo.GetUser("+111222");
        Assert.Equal(user.TLBytes!.Value.AsSpan().ToArray(), 
            userBytes!.Value.AsSpan().ToArray());
        Util.DeleteDirectory(path);
    }
    [Fact]
    public void Should_GetUserId()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        UserRepository repo = new UserRepository(new RocksDBKVStore(ctx), 
            new RocksDBKVStore(ctx),new RocksDBKVStore(ctx));
        using var user = User.Builder()
            .Id(123)
            .Username("test"u8)
            .Phone("+111222"u8)
            .FirstName("aaa"u8)
            .LastName("bbb"u8)
            .Build();
        repo.PutUser(user);
        var userId = repo.GetUserId("+111222");
        Assert.Equal(user.Id, userId);
        Util.DeleteDirectory(path);
    }
    [Fact]
    public void Should_Delete_User()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        UserRepository repo = new UserRepository(new RocksDBKVStore(ctx), 
            new RocksDBKVStore(ctx),new RocksDBKVStore(ctx));
        using var user = User.Builder()
            .Id(123)
            .Username("test"u8)
            .Phone("+111222"u8)
            .FirstName("aaa"u8)
            .LastName("bbb"u8)
            .Build();
        repo.PutUser(user);
        repo.DeleteUser(123);
        var userBytes =  repo.GetUser(123);
        Assert.Null(userBytes);
        Util.DeleteDirectory(path);
    }
    [Fact]
    public void Should_UpdateUsername()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        UserRepository repo = new UserRepository(new RocksDBKVStore(ctx), 
            new RocksDBKVStore(ctx),new RocksDBKVStore(ctx));
        using var user = User.Builder()
            .Id(123)
            .Username("test"u8)
            .Phone("+111222"u8)
            .FirstName("aaa"u8)
            .LastName("bbb"u8)
            .Build();
        repo.PutUser(user);
        repo.UpdateUsername(123, "newname");
        var userBytes =  repo.GetUser(123);
        Assert.NotNull(userBytes);
        var updated = new User(userBytes.Value.AsSpan());
        Assert.Equal(updated.Username.ToArray(), Encoding.UTF8.GetBytes("newname"));
        Util.DeleteDirectory(path);
    }
    [Fact]
    public void Should_UpdateUserphone()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        UserRepository repo = new UserRepository(new RocksDBKVStore(ctx), 
            new RocksDBKVStore(ctx),new RocksDBKVStore(ctx));
        using var user = User.Builder()
            .Id(123)
            .Username("test"u8)
            .Phone("+111222"u8)
            .FirstName("aaa"u8)
            .LastName("bbb"u8)
            .Build();
        repo.PutUser(user);
        repo.UpdateUserPhone(123, "+333444");
        var userBytes =  repo.GetUser(123);
        Assert.NotNull(userBytes);
        var updated = new User(userBytes.Value.AsSpan());
        Assert.Equal(updated.Phone.ToArray(), Encoding.UTF8.GetBytes("+333444"));
        Util.DeleteDirectory(path);
    }
    [Fact]
    public void Should_UpdateAndGetAccountTtl()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        UserRepository repo = new UserRepository(new RocksDBKVStore(ctx), 
            new RocksDBKVStore(ctx),new RocksDBKVStore(ctx));
        repo.UpdateAccountTtl(123, 200);
        var ttl = repo.GetAccountTtl(123);
        Assert.Equal(199, ttl);
        Util.DeleteDirectory(path);
    }
}