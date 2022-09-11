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
public class MemcomparableKey
{
    private readonly byte[] _key;
    public ReadOnlySpan<byte> Value => _key;
    public Span<byte> Span => _key;
    public byte[] ArrayValue => _key;
    public long ExpiresAt { get; set; }
    private MemcomparableKey(byte[] value)
    {
        _key = value;
    }
    public MemcomparableKey(string tableName)
    {
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        _key = new byte[8];
        Encode(_key, (long)hash, 0);
    }
    
    public MemcomparableKey(string tableName, int value)
    {
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        _key = new byte[12];
        Encode(_key, (long)hash, 0);
        Encode(_key, value, 8);
    }
    public MemcomparableKey(string tableName, bool value)
    {
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        byte byteValue = (byte)(value ? 1 : 0);
        _key = new byte[9];
        Encode(_key, (long)hash, 0);
        _key[8] |= byteValue;
    }
    public MemcomparableKey(string tableName, long value)
    {
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        _key = new byte[16];
        Encode(_key, (long)hash, 0);
        Encode(_key, value, 8);
    }
    public MemcomparableKey(string tableName, DateTime value)
    {
        long val = value.Ticks;
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        _key = new byte[16];
        Encode(_key, (long)hash, 0);
        Encode(_key, val, 8);
    }
    public MemcomparableKey(string tableName, DateTimeOffset value)
    {
        long val = value.Ticks;
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        _key = new byte[16];
        Encode(_key, (long)hash, 0);
        Encode(_key, val, 8);
    }
    public MemcomparableKey(string tableName, float value)
    {
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        _key = new byte[12];
        Encode(_key, (long)hash, 0);
        Encode(_key, value, 8);
    }
    public MemcomparableKey(string tableName, double value)
    {
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        _key = new byte[16];
        Encode(_key, (long)hash, 0);
        Encode(_key, value, 8);
    }
    public MemcomparableKey(string tableName, Span<byte> value)
    {
        int blocks = value.Length / 8 + 
            value.Length % 8 == 0 ? 0 : 1;
        _key = new byte[8 + blocks * 9];
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        Encode(_key, (long)hash, 0);
        for (int i = 0; i < blocks; i++)
        {
            int significantBytes = value.Length - i * 8;
            if (significantBytes > 8)
            {
                significantBytes = 9;
            }
            _key[8 + i * 9 + 8] = (byte)significantBytes;
            value.Slice(i * 8, Math.Min(significantBytes, 8))
                .CopyTo(_key.AsSpan().Slice(8 + i * 9));
        }
    }

    public MemcomparableKey(string tableName, string val)
    {
        var value = Encoding.UTF8.GetBytes(val).AsSpan();
        int blocks = value.Length / 8 + 
            value.Length % 8 == 0 ? 0 : 1;
        _key = new byte[8 + blocks * 9];
        var chars = tableName.AsSpan();
        var bytes = MemoryMarshal.Cast<char, byte>(chars);
        var hash = bytes.GetXxHash64();
        Encode(_key, (long)hash, 0);
        for (int i = 0; i < blocks; i++)
        {
            int significantBytes = value.Length - i * 8;
            if (significantBytes > 8)
            {
                significantBytes = 9;
            }
            _key[8 + i * 9 + 8] = (byte)significantBytes;
            value.Slice(i * 8, Math.Min(significantBytes, 8))
                .CopyTo(_key.AsSpan().Slice(8 + i * 9));
        }
    }
    public MemcomparableKey Append(Span<byte> value)
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

        return new MemcomparableKey(newKey);
    }
    public MemcomparableKey Append(bool value)
    {
        byte[] newKey = new byte[_key.Length + 1];
        _key.CopyTo(newKey.AsSpan());
        byte byteValue = (byte)(value ? 1 : 0);
        newKey[_key.Length] = byteValue;

        return new MemcomparableKey(newKey);
    }
    public MemcomparableKey Append(int value)
    {
        byte[] newKey = new byte[_key.Length + 4];
        _key.CopyTo(newKey.AsSpan());
        Encode(newKey, value, _key.Length);
        return new MemcomparableKey(newKey);
    }
    public MemcomparableKey Append(long value)
    {
        byte[] newKey = new byte[_key.Length + 8];
        _key.CopyTo(newKey.AsSpan());
        Encode(newKey, value, _key.Length);
        return new MemcomparableKey(newKey);
    }
    public MemcomparableKey Append(float value)
    {
        byte[] newKey = new byte[_key.Length + 4];
        _key.CopyTo(newKey.AsSpan());
        BinaryPrimitives.WriteSingleBigEndian(newKey.AsSpan().Slice(_key.Length), value);
        Encode(newKey, value, _key.Length);
        return new MemcomparableKey(newKey);
    }
    public MemcomparableKey Append(double value)
    {
        byte[] newKey = new byte[_key.Length + 4];
        _key.CopyTo(newKey.AsSpan());
        BinaryPrimitives.WriteDoubleBigEndian(newKey.AsSpan().Slice(_key.Length), value);
        Encode(newKey, value, _key.Length);
        return new MemcomparableKey(newKey);
    }
    public MemcomparableKey Append(string value)
    {
        return Append(Encoding.UTF8.GetBytes(value));
    }
    public MemcomparableKey Append(DateTime value)
    {
        return Append(value.Ticks);
    }
    public MemcomparableKey Append(DateTimeOffset value)
    {
        return Append(value.Ticks);
    }
    public static MemcomparableKey Create(string tableName, IReadOnlyCollection<object> values)
    {
        MemcomparableKey key = new MemcomparableKey(tableName);
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
    private static void Encode(byte[] key, int value, int offset)
    {
        BinaryPrimitives.WriteUInt32BigEndian(key.AsSpan(offset), (uint)value);
        if (value < 0)
        {
            for (int i = 0; i < 4; i++)
            {
                key[offset + i] = (byte)(~key[offset + i]);
            }
        }
        else
        {
            key[offset] ^= 128;
        }
    }
    private static void Encode(byte[] key, long value, int offset)
    {
        BinaryPrimitives.WriteUInt64BigEndian(key.AsSpan(offset), (ulong)value);
        if (value < 0)
        {
            for (int i = 0; i < 8; i++)
            {
                key[offset + i] = (byte)(~key[offset + i]);
            }
        }
        else
        {
            key[offset] ^= 128;
        }
    }
    private static void Encode(byte[] key, float value, int offset)
    {
        if (value == 0.0f)
        {
            key[offset] = 128;
            for (int i = 1; i < 4; i++)
            {
                key[offset + i] = 0;
            }
            return;
        }
        BinaryPrimitives.WriteSingleBigEndian(key.AsSpan(offset), (uint)value);
        if (value < 0)
        {
            for (int i = 0; i < 4; i++)
            {
                key[offset + i] = (byte)(~key[offset + i]);
            }
        }
        else
        {
            ushort exponent = (ushort)((key[0] << 8) | key[1] | 32768);
            exponent += 1 << 7;
            key[offset] = (byte)(exponent >> 8);
            key[offset + 1] = (byte)(exponent);
        }
    }
    private static void Encode(byte[] key, double value, int offset)
    {
        if (value == 0.0d)
        {
            key[offset] = 128;
            for (int i = 1; i < 8; i++)
            {
                key[offset + i] = 0;
            }
            return;
        }
        BinaryPrimitives.WriteSingleBigEndian(key.AsSpan(offset), (uint)value);
        if (value < 0)
        {
            for (int i = 0; i < 8; i++)
            {
                key[offset + i] = (byte)(~key[offset + i]);
            }
        }
        else
        {
            ushort exponent = (ushort)((key[0] << 8) | key[1] | 32768);
            exponent += 1 << 7;
            key[offset] = (byte)(exponent >> 4);
            key[offset + 1] = (byte)(exponent);
        }
    }
}