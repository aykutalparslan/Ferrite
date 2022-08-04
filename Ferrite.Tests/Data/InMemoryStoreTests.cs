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
using Ferrite.Data.Repositories;
using Nest;
using Xunit;

namespace Ferrite.Tests.Data;

public class InMemoryStoreTests
{
    [Theory]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(200)]
    async Task InMemoryStore_Exist_ConformsToExpirationRules(int ttl)
    {
        InMemoryStore store = new InMemoryStore();
        store.SetSchema(new TableDefinition("test", "keys",
            new KeyDefinition("pk",
                new DataColumn { Name = "id", Type = DataType.String })));
        store.Put(RandomNumberGenerator.GetBytes(8), ttl, "test123");
        await Task.Delay(ttl/4*3).ContinueWith(_ => Assert.True(store.Exists("test123")));
        await Task.Delay(ttl-ttl/4*2).ContinueWith(_ => Assert.False(store.Exists("test123")));
    }
    [Theory]
    [InlineData(20)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(200)]
    async Task InMemoryStore_Get_ConformsToExpirationRules(int ttl)
    {
        InMemoryStore store = new InMemoryStore();
        store.SetSchema(new TableDefinition("test", "keys",
            new KeyDefinition("pk",
                new DataColumn { Name = "id", Type = DataType.String })));
        store.Put(RandomNumberGenerator.GetBytes(8), ttl, "test123");
        await Task.Delay(ttl/4*3).ContinueWith(_ => Assert.NotNull(store.Get("test123")));
        await Task.Delay(ttl-ttl/4*2).ContinueWith(_ => Assert.Null(store.Get("test123")));
    }
    [Fact]
    async Task InMemoryStore_Get_ReturnsValue()
    {
        InMemoryStore store = new InMemoryStore();
        store.SetSchema(new TableDefinition("test", "keys",
            new KeyDefinition("pk",
                new DataColumn { Name = "id", Type = DataType.String })));
        var value = RandomNumberGenerator.GetBytes(8);
        store.Put(value, 50, "test123");
        Assert.Equal(value, store.Get("test123"));
    }
}