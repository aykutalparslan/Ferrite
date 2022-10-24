//
//    Project Ferrite is an Implementation Telegram Server API
//    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Affero General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using System.Buffers;
using DotNext.IO;
using Ferrite.TL.Exceptions;
using Ferrite.TL.slim;

namespace Ferrite.TL
{
    public struct Bool : ITLObject
    {
        public bool Value { get; set; }
        private static readonly ReadOnlySequence<byte> trueBytes;
        private static readonly ReadOnlySequence<byte> falseBytes;
        static Bool()
        {
            byte[] v = new byte[4];
            int c = unchecked((int)0x997275b5);
            v[0] = (byte)(c & 0xff);
            v[1] = (byte)(c >> 8 & 0xff);
            v[2] = (byte)(c >> 16 & 0xff);
            v[3] = (byte)(c >> 24 & 0xff);
            trueBytes = new ReadOnlySequence<byte>(v);

            c = unchecked((int)0xbc799737);
            v[0] = (byte)(c & 0xff);
            v[1] = (byte)(c >> 8 & 0xff);
            v[2] = (byte)(c >> 16 & 0xff);
            v[3] = (byte)(c >> 24 & 0xff);
            falseBytes = new ReadOnlySequence<byte>(v);
        }

        public int Constructor => Value ? unchecked((int)0x997275b5):
                                          unchecked((int)0xbc799737);

        public ReadOnlySequence<byte> TLBytes => Value ? trueBytes:
                                                         falseBytes;

        public void Parse(ref SequenceReader buff)
        {
            int c = buff.ReadInt32(true);
            if(c == unchecked((int)0x997275b5))
            {
                Value = true;
            }
            else if(c == unchecked((int)0xbc799737))
            {
                Value = false;
            }
            else
            {
                throw new DeserializationException();
            }
        }

        public static bool Read(ref SequenceReader buff)
        {
            int c = buff.ReadInt32(true);
            if (c == unchecked((int)0x997275b5))
            {
                return true;
            }
            else if (c == unchecked((int)0xbc799737))
            {
                return false;
            }
            else
            {
                throw new DeserializationException();
            }
        }

        public static int GetConstructor(bool value)
        {
            if (value)
            {
                return unchecked((int)0x997275b5);
            }
            else
            {
                return unchecked((int)0xbc799737);
            }
        }

        public void WriteTo(Span<byte> buff)
        {
            if (Value)
            {
                trueBytes.CopyTo(buff);
            }
            else
            {
                falseBytes.CopyTo(buff);
            }
        }
    }
}

