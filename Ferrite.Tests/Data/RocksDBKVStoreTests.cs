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
using Ferrite.Data;
using Ferrite.Data.Repositories;
using Xunit;

namespace Ferrite.Tests.Data;

public class RocksDBKVStoreTests
{
    [Fact]
    public void RocksDBKVStore_Should_PutAndGet()
    {
        string path = "test-" + Random.Shared.Next();
        using RocksDBContext context = new RocksDBContext(path);
        RocksDBKVStore store = new RocksDBKVStore(context);
        var table = new TableDefinition("test", "test",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "phone_number", Type = DataType.String }),
            new KeyDefinition("by_phone",
                new DataColumn { Name = "phone_number", Type = DataType.String }));
        store.SetSchema(table);
        byte[] expected = RandomNumberGenerator.GetBytes(128);
        store.Put(expected, 123L, "5554443322");
        Assert.Equal(expected, store.Get(123L));
        Assert.Equal(expected, store.GetBySecondaryIndex("by_phone", "5554443322"));
        if(Directory.Exists(path)) Directory.Delete(path, true);
    }
    [Fact]
    public void RocksDBKVStore_Should_Iterate()
    {
        string path = "test-" + Random.Shared.Next();
        using RocksDBContext context = new RocksDBContext(path);
        RocksDBKVStore store = new RocksDBKVStore(context);
        var table = new TableDefinition("test", "test",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "phone_number", Type = DataType.String }),
            new KeyDefinition("by_phone",
                new DataColumn { Name = "phone_number", Type = DataType.String }));
        store.SetSchema(table);
        List<byte[]> records = new();
        for (int i = 0; i < 10; i++)
        {
            byte[] b = RandomNumberGenerator.GetBytes(128);
            records.Add(b);
        }

        int num = 0;
        foreach (var r in records)
        {
            store.Put(r, 123L, "555444332" + num++);
        }

        var iterator = store.Iterate(123L);
        int count = 0;
        foreach (var b in iterator)
        {
            Assert.Equal(records[count], b);
            count++;
        }
        
        if(Directory.Exists(path)) Directory.Delete(path, true);
    }
    [Fact]
    public void RocksDBKVStore_Should_DeleteSecondaryKeys()
    {
        string path = "test-" + Random.Shared.Next();
        using RocksDBContext context = new RocksDBContext(path);
        RocksDBKVStore store = new RocksDBKVStore(context);
        var table = new TableDefinition("test", "test",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "phone_number", Type = DataType.String }),
            new KeyDefinition("by_phone",
                new DataColumn { Name = "phone_number", Type = DataType.String }));
        store.SetSchema(table);
        store.Put(RandomNumberGenerator.GetBytes(8), 123L, "5554443322");
        var secondaryKey = MemcomparableKey.Create(table.SecondaryIndices[0].FullName, new []{"5554443322"});
        Assert.NotNull(context.Get(secondaryKey.Value));
        store.Delete(123L, "5554443322");
        Assert.Null(context.Get(secondaryKey.Value));
        store.Put(RandomNumberGenerator.GetBytes(8), 444L, "4443332211");
        secondaryKey = MemcomparableKey.Create(table.SecondaryIndices[0].FullName, new []{"4443332211"});
        Assert.NotNull(context.Get(secondaryKey.Value));
        store.Delete(444L);
        Assert.Null(context.Get(secondaryKey.Value));
        if(Directory.Exists(path)) Directory.Delete(path, true);
    }
    [Fact]
    public void RocksDBKVStore_Should_Delete()
    {
        string path = "test-" + Random.Shared.Next();
        using RocksDBContext context = new RocksDBContext(path);
        RocksDBKVStore store = new RocksDBKVStore(context);
        var table = new TableDefinition("test", "test",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "phone_number", Type = DataType.String }),
            new KeyDefinition("by_phone",
                new DataColumn { Name = "phone_number", Type = DataType.String }));
        store.SetSchema(table);
        store.Put(RandomNumberGenerator.GetBytes(8), 123L, "5554443322");
        var primaryKey = new MemcomparableKey(table.FullName,  123L).Append("5554443322");
        Assert.NotNull(context.Get(primaryKey.Value));
        store.Delete(123L, "5554443322");
        Assert.Null(context.Get(primaryKey.Value));
        store.Put(RandomNumberGenerator.GetBytes(8), 123L, "5554443323");
        store.Put(RandomNumberGenerator.GetBytes(8), 123L, "5554443324");
        store.Put(RandomNumberGenerator.GetBytes(8), 123L, "5554443325");
        store.Put(RandomNumberGenerator.GetBytes(8), 123L, "5554443326");
        primaryKey = new MemcomparableKey(table.FullName,  123L);
        var iter = context.Iterate(primaryKey.ArrayValue);
        int count = 0;
        foreach (var v in iter)
        {
            count++;
        }
        Assert.Equal(4, count);
        store.Delete(123L);
        iter = context.Iterate(primaryKey.ArrayValue);
        count = 0;
        foreach (var v in iter)
        {
            count++;
        }
        Assert.Equal(0, count);
        if(Directory.Exists(path)) Directory.Delete(path, true);
    }
    [Fact]
    public void RocksDBKVStore_Should_DeleteBySecondaryIndex()
    {
        string path = "test-" + Random.Shared.Next();
        using RocksDBContext context = new RocksDBContext(path);
        RocksDBKVStore store = new RocksDBKVStore(context);
        var table = new TableDefinition("test", "test",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "phone_number", Type = DataType.String }),
            new KeyDefinition("by_phone",
                new DataColumn { Name = "phone_number", Type = DataType.String }));
        store.SetSchema(table);
        store.Put(RandomNumberGenerator.GetBytes(8), 123L, "5554443322");
        var primaryKey = new MemcomparableKey(table.FullName,  123L).Append("5554443322");
        var secondaryKey = new MemcomparableKey(table.SecondaryIndices[0].FullName, "5554443322");
        Assert.NotNull(context.Get(primaryKey.Value));
        Assert.NotNull(context.Get(secondaryKey.Value));
        //store.Delete(123L, "5554443322");
        store.DeleteBySecondaryIndex("by_phone", "5554443322");
        Assert.Null(context.Get(primaryKey.Value));
        Assert.Null(context.Get(secondaryKey.Value));
        if(Directory.Exists(path)) Directory.Delete(path, true);
    }
}