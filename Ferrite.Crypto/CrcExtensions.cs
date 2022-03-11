/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System.Buffers;

namespace Ferrite.Crypto;

public static class CrcExtensions
{
    static uint[] table = new uint[256];
    static CrcExtensions()
    {
        table[0] = 0;
        uint crc;
        for(uint i = 0; i < 256; i++)
        {
            crc = i;
            for (uint j = 0; j < 8; j++)
            {
                var tmp = crc & 1;
                crc = tmp == 1 ? 0xEDB88320 ^ (crc >> 1) : (crc >> 1);
                table[i] = crc;
            }
        }
    }
    public static uint GetCrc32(this ReadOnlySequence<byte> bytes)
    {
        uint crc32 = 0xFFFFFFFFu;
        uint index = 0;
        var pos = bytes.Start;
        foreach (var b in bytes)
        {
            index = (crc32 ^ b.Span[0]) & 0xff;
            crc32 = (crc32 >> 8) ^ table[index];
        }
        
        crc32 ^= 0xFFFFFFFFu;
        return crc32;
    }
    public static uint GetCrc32(this ReadOnlySpan<byte> bytes)
    {
        uint crc32 = 0xFFFFFFFFu;
        uint index = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            index = (crc32 ^ bytes[i]) & 0xff;
            crc32 = (crc32 >> 8) ^ table[index];
        }
        crc32 ^= 0xFFFFFFFFu;
        return crc32;
    }
    public static uint GetCrc32(this Span<byte> bytes)
    {
        uint crc32 = 0xFFFFFFFFu;
        uint index = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            index = (crc32 ^ bytes[i]) & 0xff;
            crc32 = (crc32 >> 8) ^ table[index];
        }
        crc32 ^= 0xFFFFFFFFu;
        return crc32;
    }
}


