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
using Xunit;

namespace Ferrite.Tests.Data;

public class MemcomparableKeyTests
{
    [Fact]
    public void Key_Should_WriteAndReadBool()
    {
        KeyDefinition definition = new KeyDefinition("test",
            new DataColumn() { Name = "field1", Type = DataType.Bool },
            new DataColumn() { Name = "field2", Type = DataType.Bool },
            new DataColumn() { Name = "field3", Type = DataType.Bool },
            new DataColumn() { Name = "field4", Type = DataType.Bool },
            new DataColumn() { Name = "field5", Type = DataType.Bool });
        MemcomparableKey key = new MemcomparableKey("test", true)
            .Append(false)
            .Append(true)
            .Append(true)
            .Append(false);
        Assert.True(key.GetBool(definition, "field1"));
        Assert.False(key.GetBool(definition, "field2"));
        Assert.True(key.GetBool(definition, "field3"));
        Assert.True(key.GetBool(definition, "field4"));
        Assert.False(key.GetBool(definition, "field5"));
        Assert.Equal(null, key.GetInt32(definition, "fieldXXX"));
    }
    [Fact]
    public void Key_Should_WriteAndReadInt32()
    {
        KeyDefinition definition = new KeyDefinition("test",
            new DataColumn() { Name = "field1", Type = DataType.Int },
            new DataColumn() { Name = "field2", Type = DataType.Int },
            new DataColumn() { Name = "field3", Type = DataType.Int },
            new DataColumn() { Name = "field4", Type = DataType.Int },
            new DataColumn() { Name = "field5", Type = DataType.Int });
        MemcomparableKey key = new MemcomparableKey("test", 13579)
            .Append(111)
            .Append(-4)
            .Append(-11199999)
            .Append(0);
        Assert.Equal(13579, key.GetInt32(definition, "field1"));
        Assert.Equal(111, key.GetInt32(definition, "field2"));
        Assert.Equal(-4, key.GetInt32(definition, "field3"));
        Assert.Equal(-11199999, key.GetInt32(definition, "field4"));
        Assert.Equal(0, key.GetInt32(definition, "field5"));
        Assert.Equal(null, key.GetInt32(definition, "fieldXXX"));
    }
    [Fact]
    public void Key_Should_WriteAndReadInt64()
    {
        KeyDefinition definition = new KeyDefinition("test",
            new DataColumn() { Name = "field1", Type = DataType.Long },
            new DataColumn() { Name = "field2", Type = DataType.Long },
            new DataColumn() { Name = "field3", Type = DataType.Long },
            new DataColumn() { Name = "field4", Type = DataType.Long },
            new DataColumn() { Name = "field5", Type = DataType.Long });
        MemcomparableKey key = new MemcomparableKey("test", 15656565663579)
            .Append(111L)
            .Append(-4L)
            .Append(-111992387942379999L)
            .Append(0L);
        Assert.Equal(15656565663579L, key.GetInt64(definition, "field1"));
        Assert.Equal(111, key.GetInt64(definition, "field2"));
        Assert.Equal(-4, key.GetInt64(definition, "field3"));
        Assert.Equal(-111992387942379999L, key.GetInt64(definition, "field4"));
        Assert.Equal(0, key.GetInt64(definition, "field5"));
        Assert.Equal(null, key.GetInt64(definition, "fieldXXX"));
    }
    [Fact]
    public void Key_Should_WriteAndReadFloat()
    {
        KeyDefinition definition = new KeyDefinition("test",
            new DataColumn() { Name = "field1", Type = DataType.Float },
            new DataColumn() { Name = "field2", Type = DataType.Float },
            new DataColumn() { Name = "field3", Type = DataType.Float },
            new DataColumn() { Name = "field4", Type = DataType.Float },
            new DataColumn() { Name = "field5", Type = DataType.Float });
        MemcomparableKey key = new MemcomparableKey("test", 1.12341234f)
            .Append(1.4f)
            .Append(-1.6f)
            .Append(-1.65757f)
            .Append(0.0f);
        Assert.Equal(1.12341234f, key.GetSingle(definition, "field1"));
        Assert.Equal(1.4f, key.GetSingle(definition, "field2"));
        Assert.Equal(-1.6f, key.GetSingle(definition, "field3"));
        Assert.Equal(-1.65757f, key.GetSingle(definition, "field4"));
        Assert.Equal(0.0f, key.GetSingle(definition, "field5"));
        Assert.Equal(null, key.GetSingle(definition, "fieldXXX"));
    }
    [Fact]
    public void Key_Should_WriteAndReadDouble()
    {
        KeyDefinition definition = new KeyDefinition("test",
            new DataColumn() { Name = "field1", Type = DataType.Double },
            new DataColumn() { Name = "field2", Type = DataType.Double },
            new DataColumn() { Name = "field3", Type = DataType.Double },
            new DataColumn() { Name = "field4", Type = DataType.Double },
            new DataColumn() { Name = "field5", Type = DataType.Double });
        MemcomparableKey key = new MemcomparableKey("test", 1.12341234d)
            .Append(1.4d)
            .Append(-1.6d)
            .Append(-1.65757d)
            .Append(0.0d);
        Assert.Equal(1.12341234d, key.GetDouble(definition, "field1"));
        Assert.Equal(1.4d, key.GetDouble(definition, "field2"));
        Assert.Equal(-1.6d, key.GetDouble(definition, "field3"));
        Assert.Equal(-1.65757d, key.GetDouble(definition, "field4"));
        Assert.Equal(0.0d, key.GetDouble(definition, "field5"));
        Assert.Equal(null, key.GetDouble(definition, "fieldXXX"));
    }
    [Fact]
    public void Key_Should_WriteAndReadBytes()
    {
        KeyDefinition definition = new KeyDefinition("test",
            new DataColumn() { Name = "field1", Type = DataType.Bytes },
            new DataColumn() { Name = "field2", Type = DataType.Bytes },
            new DataColumn() { Name = "field3", Type = DataType.Bytes },
            new DataColumn() { Name = "field4", Type = DataType.Bytes },
            new DataColumn() { Name = "field5", Type = DataType.Bytes });
        MemcomparableKey key = new MemcomparableKey("test", Encoding.UTF8.GetBytes("hello"))
            .Append(Encoding.UTF8.GetBytes("asdlşkfhalskdhflaskdhjfalshf"))
            .Append(Encoding.UTF8.GetBytes("asdkfjhaskdhf"))
            .Append(Encoding.UTF8.GetBytes("ddddd"))
            .Append(Array.Empty<byte>());
        Assert.Equal(Encoding.UTF8.GetBytes("hello"), key.GetBytes(definition, "field1"));
        Assert.Equal(Encoding.UTF8.GetBytes("asdlşkfhalskdhflaskdhjfalshf"), key.GetBytes(definition, "field2"));
        Assert.Equal(Encoding.UTF8.GetBytes("asdkfjhaskdhf"), key.GetBytes(definition, "field3"));
        Assert.Equal(Encoding.UTF8.GetBytes("ddddd"), key.GetBytes(definition, "field4"));
        Assert.Equal(Array.Empty<byte>(), key.GetBytes(definition, "field5"));
        Assert.Equal(null, key.GetBytes(definition, "fieldXXX"));
    }
    [Fact]
    public void Key_Should_WriteAndReadString()
    {
        KeyDefinition definition = new KeyDefinition("test",
            new DataColumn() { Name = "field1", Type = DataType.String },
            new DataColumn() { Name = "field2", Type = DataType.String },
            new DataColumn() { Name = "field3", Type = DataType.String },
            new DataColumn() { Name = "field4", Type = DataType.String },
            new DataColumn() { Name = "field5", Type = DataType.String });
        MemcomparableKey key = new MemcomparableKey("test", "sdfgsdfgsdfgsdfg k sjdhfg ksdjhfg ksd")
            .Append("asdlşkfhalskdhflaskdhjfalshf")
            .Append("asdkfjhaskdhf")
            .Append("ddddd")
            .Append("");
        Assert.Equal("sdfgsdfgsdfgsdfg k sjdhfg ksdjhfg ksd", key.GetString(definition, "field1"));
        Assert.Equal("asdlşkfhalskdhflaskdhjfalshf", key.GetString(definition, "field2"));
        Assert.Equal("asdkfjhaskdhf", key.GetString(definition, "field3"));
        Assert.Equal("ddddd", key.GetString(definition, "field4"));
        Assert.Equal("", key.GetString(definition, "field5"));
        Assert.Equal(null, key.GetBytes(definition, "fieldXXX"));
    }
    [Fact]
    public void Key_Should_WriteAndReadValues()
    {
        KeyDefinition definition = new KeyDefinition("test",
            new DataColumn() { Name = "field1", Type = DataType.Long },
            new DataColumn() { Name = "field2", Type = DataType.String },
            new DataColumn() { Name = "field3", Type = DataType.Int },
            new DataColumn() { Name = "field4", Type = DataType.Float },
            new DataColumn() { Name = "field5", Type = DataType.Double },
            new DataColumn() { Name = "field6", Type = DataType.Bytes });
        MemcomparableKey key = new MemcomparableKey("test", 122221341234233L)
            .Append("hello")
            .Append(12345)
            .Append(234.2345f)
            .Append(2341.1234123444444d)
            .Append(Encoding.UTF8.GetBytes("skdmfhskdajfkasdfkasjfakjsdhfaksdhfkashf"));
        Assert.Equal(122221341234233L, key.GetInt64(definition, "field1"));
        Assert.Equal("hello", key.GetString(definition, "field2"));
        Assert.Equal(12345, key.GetInt32(definition, "field3"));
        Assert.Equal(234.2345f, key.GetSingle(definition, "field4"));
        Assert.Equal(2341.1234123444444d, key.GetDouble(definition, "field5"));
        Assert.Equal(Encoding.UTF8.GetBytes("skdmfhskdajfkasdfkasjfakjsdhfaksdhfkashf"), key.GetBytes(definition, "field6"));
        Assert.Equal(null, key.GetBytes(definition, "fieldXXX"));
    }
}