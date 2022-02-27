/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
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


