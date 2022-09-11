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
using Ferrite.Data;
using Ferrite.Data.Repositories;
using RocksDbSharp;
using Xunit;

namespace Ferrite.Tests.Data;

public class RocksDBContextTests
{
    [Fact]
    public void RocksDBContext_Should_PutAndGet()
    {
        string path = "test-" + Random.Shared.Next();
        RocksDBContext context = new RocksDBContext(path);
        List<MemcomparableKey> keys = new();
        Dictionary<byte[],byte[]> values = new();
        for(int i = 0; i < 10; i++)
        {
            MemcomparableKey key = new MemcomparableKey("test", 111);
            key = key.Append(i);
            var value = Encoding.UTF8.GetBytes("aaa-" + i);
            values.Add(key.ArrayValue, value);
            context.Put(key.Value, value);
        }

        foreach (var key in keys)
        {
            Assert.Equal(values[key.ArrayValue], context.Get(key.Span));
        }
        context.Dispose();
        if(Directory.Exists(path)) Directory.Delete(path, true);
    }
    [Fact]
    public void RocksDBContext_Should_PutDeleteAndGet()
    {
        string path = "test-" + Random.Shared.Next();
        RocksDBContext context = new RocksDBContext(path);
        List<MemcomparableKey> keys = new();
        Dictionary<byte[],byte[]> values = new();
        for(int i = 0; i < 10; i++)
        {
            MemcomparableKey key = new MemcomparableKey("test", 111);
            key = key.Append(i);
            keys.Add(key);
            var value = Encoding.UTF8.GetBytes("aaa-" + i);
            values.Add(key.ArrayValue, value);
            context.Put(key.Value, value);
        }
        
        for(int i = 0; i < 5; i++)
        {
            context.Delete(keys[i].Span);
        }
        for(int i = 0; i < 5; i++)
        {
            Assert.Equal(null, context.Get(keys[i].Span));
        }
        for(int i = 5; i < 10; i++)
        {
            Assert.Equal(values[keys[i].ArrayValue], context.Get(keys[i].Span));
        }
        context.Dispose();
        if(Directory.Exists(path)) Directory.Delete(path, true);
    }
    [Fact]
    public void RocksDBContext_Should_PutDeleteRangeAndGet()
    {
        string path = "test-" + Random.Shared.Next();
        RocksDBContext context = new RocksDBContext(path);
        List<MemcomparableKey> keys1 = new();
        List<MemcomparableKey> keys2 = new();
        List<MemcomparableKey> keys3 = new();
        for(int i = 0; i < 10; i++)
        {
            MemcomparableKey key = new MemcomparableKey("test", 111);
            key = key.Append(i);
            keys1.Add(key);
            var value = Encoding.UTF8.GetBytes("aaa-" + i);
            context.Put(key.Value, value);
        }
        for(int i = 0; i < 10; i++)
        {
            MemcomparableKey key = new MemcomparableKey("test", 222);
            key = key.Append(i);
            keys2.Add(key);
            var value = Encoding.UTF8.GetBytes("bbb-" + i);
            context.Put(key.Value, value);
        }
        for(int i = 0; i < 10; i++)
        {
            MemcomparableKey key = new MemcomparableKey("test")
                .Append("username")
                .Append(21341234L)
                .Append(i);
            keys3.Add(key);
            var value = Encoding.UTF8.GetBytes("bbb-" + i);
            context.Put(key.Value, value);
        }
        MemcomparableKey keyDelete = new MemcomparableKey("test", 111);
        context.DeleteWithPrefix(keyDelete.ArrayValue);
        foreach (var key in keys1)
        {
            Assert.Null(context.Get(key.Span));
        }
        foreach (var key in keys2)
        {
            Assert.NotNull(context.Get(key.Span));
        }
        foreach (var key in keys3)
        {
            Assert.NotNull(context.Get(key.Span));
        }
        context.Dispose();
        if(Directory.Exists(path)) Directory.Delete(path, true);
    }
    [Fact]
    public void RocksDBContext_Should_PutDeleteRangeAndIterate()
    {
        string path = "test-" + Random.Shared.Next();
        RocksDBContext context = new RocksDBContext(path);
        List<MemcomparableKey> keys1 = new();
        List<MemcomparableKey> keys2 = new();
        List<MemcomparableKey> keys3 = new();
        for(int i = 0; i < 10; i++)
        {
            MemcomparableKey key = new MemcomparableKey("test", 111);
            key = key.Append(i);
            keys1.Add(key);
            var value = Encoding.UTF8.GetBytes("aaa-" + i);
            context.Put(key.Value, value);
        }
        for(int i = 0; i < 10; i++)
        {
            MemcomparableKey key = new MemcomparableKey("test", 222);
            key = key.Append(i);
            keys2.Add(key);
            var value = Encoding.UTF8.GetBytes("bbb-" + i);
            context.Put(key.Value, value);
        }
        for(int i = 0; i < 10; i++)
        {
            MemcomparableKey key = new MemcomparableKey("test")
                .Append("username")
                .Append(21341234L)
                .Append(i);
            keys3.Add(key);
            var value = Encoding.UTF8.GetBytes("bbb-" + i);
            context.Put(key.Value, value);
        }
        for(int i = 0; i < 10; i++)
        {
            MemcomparableKey key = new MemcomparableKey("test2")
                .Append(21341234L)
                .Append("username")
                .Append(i);
            keys3.Add(key);
            var value = Encoding.UTF8.GetBytes("bbb-" + i);
            context.Put(key.Value, value);
        }
        MemcomparableKey keyDelete = new MemcomparableKey("test", 111);
        context.DeleteWithPrefix(keyDelete.ArrayValue);
        MemcomparableKey keyIterate = new MemcomparableKey("test");
        var iter = context.Iterate(keyIterate.ArrayValue);
        int count = 0;
        foreach (var v in iter)
        {
            count++;
        }
        Assert.Equal(20, count);
        keyIterate = new MemcomparableKey("test")
            .Append("username");
        iter = context.Iterate(keyIterate.ArrayValue);
        count = 0;
        foreach (var v in iter)
        {
            count++;
        }
        Assert.Equal(10, count);
        keyIterate = new MemcomparableKey("test2")
            .Append(21341234L);
        iter = context.Iterate(keyIterate.ArrayValue);
        count = 0;
        foreach (var v in iter)
        {
            count++;
        }
        Assert.Equal(10, count);
        context.Dispose();
        if(Directory.Exists(path)) Directory.Delete(path, true);
    }
}