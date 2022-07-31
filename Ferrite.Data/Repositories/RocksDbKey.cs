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

using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using xxHash;

namespace Ferrite.Data;

//encodes multi column keys to a single key using the memcomparable format
//here: https://github.com/facebook/mysql-5.6/wiki/MyRocks-record-format#memcomparable-format
//documentation license: Creative Commons Attribution 4.0 International Public License
public readonly struct RocksDbKey
{
    private readonly byte[] _key;
    public ReadOnlySpan<byte> Value => _key;
    public byte[] ArrayValue => _key;
    private RocksDbKey(byte[] value)
    {
        _key = value;
    }
    public RocksDbKey(string tableName)
    {
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        _key = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(_key, (uint)hash);
    }
    public RocksDbKey(string tableName, int value)
    {
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        _key = new byte[8];
        BinaryPrimitives.WriteUInt32BigEndian(_key, (uint)hash);
        BinaryPrimitives.WriteUInt32BigEndian(_key.AsSpan().Slice(4), (uint)value);
        if (value < 0)
        {
            for (int i = 4; i < _key.Length; i++)
            {
                _key[i] = (byte)(~_key[i]);
            }
        }
        else
        {
            _key[4] |= 0x80;
        }
    }
    public RocksDbKey(string tableName, bool value)
    {
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        byte byteValue = (byte)(value ? 1 : 0);
        _key = new byte[5];
        BinaryPrimitives.WriteUInt32BigEndian(_key, (uint)hash);
        _key[4] |= byteValue;
    }
    public RocksDbKey(string tableName, long value)
    {
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        _key = new byte[12];
        BinaryPrimitives.WriteUInt32BigEndian(_key, (uint)hash);
        BinaryPrimitives.WriteUInt64BigEndian(_key.AsSpan().Slice(4), (ulong)value);
        if (value < 0)
        {
            for (int i = 4; i < _key.Length; i++)
            {
                _key[i] = (byte)(~_key[i]);
            }
        }
        else
        {
            _key[4] |= 0x80;
        }
    }
    public RocksDbKey(string tableName, DateTime value)
    {
        long val = value.Ticks;
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        _key = new byte[12];
        BinaryPrimitives.WriteUInt32BigEndian(_key, (uint)hash);
        BinaryPrimitives.WriteUInt64BigEndian(_key.AsSpan().Slice(4), (ulong)val);
        if (val < 0)
        {
            for (int i = 4; i < _key.Length; i++)
            {
                _key[i] = (byte)(~_key[i]);
            }
        }
        else
        {
            _key[4] |= 0x80;
        }
    }
    public RocksDbKey(string tableName, DateTimeOffset value)
    {
        long val = value.Ticks;
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        _key = new byte[12];
        BinaryPrimitives.WriteUInt32BigEndian(_key, (uint)hash);
        BinaryPrimitives.WriteUInt64BigEndian(_key.AsSpan().Slice(4), (ulong)val);
        if (val < 0)
        {
            for (int i = 4; i < _key.Length; i++)
            {
                _key[i] = (byte)(~_key[i]);
            }
        }
        else
        {
            _key[4] |= 0x80;
        }
    }
    public RocksDbKey(string tableName, float value)
    {
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        _key = new byte[8];
        BinaryPrimitives.WriteUInt32BigEndian(_key, (uint)hash);
        BinaryPrimitives.WriteSingleBigEndian(_key.AsSpan().Slice(4), value);
        if (value < 0)
        {
            for (int i = 4; i < _key.Length; i++)
            {
                _key[i] = (byte)(~_key[i]);
            }
        }
        else
        {
            _key[4] |= 0x80;
            _key[7]++;
        }
    }
    public RocksDbKey(string tableName, double value)
    {
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        _key = new byte[8];
        BinaryPrimitives.WriteUInt32BigEndian(_key, (uint)hash);
        BinaryPrimitives.WriteDoubleBigEndian(_key.AsSpan().Slice(4), value);
        if (value < 0)
        {
            for (int i = 4; i < _key.Length; i++)
            {
                _key[i] = (byte)(~_key[i]);
            }
        }
        else
        {
            _key[4] |= 0x80;
            _key[11]++;
        }
    }
    public RocksDbKey(string tableName, Span<byte> value)
    {
        int blocks = value.Length / 8 + 
            value.Length % 8 == 0 ? 0 : 1;
        _key = new byte[4 + value.Length + blocks * 9];
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        BinaryPrimitives.WriteUInt32BigEndian(_key, (uint)hash);
        for (int i = 0; i < blocks; i++)
        {
            int significantBytes = value.Length - i * 8;
            if (significantBytes > 8)
            {
                significantBytes = 9;
            }
            _key[4 + i * 9 + 8] = (byte)significantBytes;
            value.Slice(i * 8, Math.Min(significantBytes, 8))
                .CopyTo(_key.AsSpan().Slice(4 + i * 9));
        }
    }
    public RocksDbKey Append(Span<byte> value)
    {
        int blocks = value.Length / 8 + 
            value.Length % 8 == 0 ? 0 : 1;
        byte[] newKey = new byte[_key.Length + blocks * 9];
        _key.CopyTo(newKey.AsSpan());
        for (int i = 0; i < blocks; i++)
        {
            int significantBytes = value.Length - i * 8;
            if (significantBytes > 8)
            {
                significantBytes = 9;
            }
            newKey[_key.Length + i * 9 + 8] = (byte)significantBytes;
            value.Slice(i * 8, Math.Min(significantBytes, 8))
                .CopyTo(newKey.AsSpan().Slice(_key.Length + i * 9));
        }

        return new RocksDbKey(newKey);
    }
    public RocksDbKey Append(bool value)
    {
        byte[] newKey = new byte[_key.Length + 1];
        _key.CopyTo(newKey.AsSpan());
        byte byteValue = (byte)(value ? 1 : 0);
        newKey[_key.Length] = byteValue;

        return new RocksDbKey(newKey);
    }
    public RocksDbKey Append(int value)
    {
        byte[] newKey = new byte[_key.Length + 4];
        _key.CopyTo(newKey.AsSpan());
        BinaryPrimitives.WriteUInt32BigEndian(newKey.AsSpan().Slice(_key.Length), (uint)value);
        if (value < 0)
        {
            for (int i = _key.Length; i < newKey.Length; i++)
            {
                newKey[i] = (byte)(~newKey[i]);
            }
        }
        else
        {
            newKey[_key.Length] |= 0x80;
        }
        return new RocksDbKey(newKey);
    }
    public RocksDbKey Append(long value)
    {
        byte[] newKey = new byte[_key.Length + 8];
        _key.CopyTo(newKey.AsSpan());
        BinaryPrimitives.WriteUInt64BigEndian(newKey.AsSpan().Slice(_key.Length), (ulong)value);
        if (value < 0)
        {
            for (int i = _key.Length; i < newKey.Length; i++)
            {
                newKey[i] = (byte)(~newKey[i]);
            }
        }
        else
        {
            newKey[_key.Length] |= 0x80;
        }
        return new RocksDbKey(newKey);
    }
    public RocksDbKey Append(float value)
    {
        byte[] newKey = new byte[_key.Length + 4];
        _key.CopyTo(newKey.AsSpan());
        BinaryPrimitives.WriteSingleBigEndian(newKey.AsSpan().Slice(_key.Length), value);
        if (value < 0)
        {
            for (int i = _key.Length; i < newKey.Length; i++)
            {
                newKey[i] = (byte)(~newKey[i]);
            }
        }
        else
        {
            newKey[_key.Length] |= 0x80;
            newKey[_key.Length + 3]++;
        }
        return new RocksDbKey(newKey);
    }
    public RocksDbKey Append(double value)
    {
        byte[] newKey = new byte[_key.Length + 4];
        _key.CopyTo(newKey.AsSpan());
        BinaryPrimitives.WriteDoubleBigEndian(newKey.AsSpan().Slice(_key.Length), value);
        if (value < 0)
        {
            for (int i = _key.Length; i < newKey.Length; i++)
            {
                newKey[i] = (byte)(~newKey[i]);
            }
        }
        else
        {
            newKey[_key.Length] |= 0x80;
            newKey[_key.Length + 7]++;
        }
        return new RocksDbKey(newKey);
    }
    public RocksDbKey Append(string value)
    {
        return Append(Encoding.UTF8.GetBytes(value));
    }
    public RocksDbKey Append(DateTime value)
    {
        return Append(value.Ticks);
    }
    public RocksDbKey Append(DateTimeOffset value)
    {
        return Append(value.Ticks);
    }
    public static RocksDbKey Create(string tableName, IReadOnlyCollection<object> values)
    {
        RocksDbKey key = new RocksDbKey(tableName);
        foreach (var v in values)
        {
            if (v is int intValue)
            {
                key = key.Append(intValue);
            }
            else if (v is bool boolValue)
            {
                key = key.Append(boolValue);
            }
            else if (v is long longValue)
            {
                key = key.Append(longValue);
            }
            else if (v is float floatValue)
            {
                key = key.Append(floatValue);
            }
            else if (v is double doubleValue)
            {
                key = key.Append(doubleValue);
            }
            else if (v is string stringValue)
            {
                key = key.Append(stringValue);
            }
            else if (v is DateTime dateTimeValue)
            {
                key = key.Append(dateTimeValue);
            }
            else if (v is DateTimeOffset dateTimeOffsetValue)
            {
                key = key.Append(dateTimeOffsetValue);
            }
            else if (v is byte[] bytesValue)
            {
                key = key.Append(bytesValue);
            }
        }

        return key;
    }
}