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

public class IncrementalCrc32
{
    static readonly uint[] Table = new uint[256];
    static IncrementalCrc32()
    {
        Table[0] = 0;
        uint crc;
        for(uint i = 0; i < 256; i++)
        {
            crc = i;
            for (uint j = 0; j < 8; j++)
            {
                var tmp = crc & 1;
                crc = tmp == 1 ? 0xEDB88320 ^ (crc >> 1) : (crc >> 1);
                Table[i] = crc;
            }
        }
    }

    private uint _crc32 = 0xFFFFFFFFu;
    private uint _index = 0;
    public uint Crc32 => _crc32 ^= 0xFFFFFFFFu;
    public IncrementalCrc32()
    {
        
    }
    public void AppendData(ReadOnlySequence<byte> bytes)
    {
        foreach (var m in bytes)
        {
            foreach (var b in m.Span)
            {
                _index = (_crc32 ^ b) & 0xff;
                _crc32 = (_crc32 >> 8) ^ Table[_index];
            }
        }
    }
}


