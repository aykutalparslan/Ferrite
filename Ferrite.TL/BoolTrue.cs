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

using System;
using System.Buffers;
using DotNext.Buffers;
using DotNext.IO;

namespace Ferrite.TL
{
    public class BoolTrue : ITLObject
    {
        private static readonly ReadOnlySequence<byte> value; 
        static BoolTrue()
        {
            byte[] v = new byte[4];
            int c = unchecked((int)0x997275b5);
            v[0] = (byte)(c & 0xff);
            v[1] = (byte)(c >> 8 & 0xff);
            v[2] = (byte)(c >> 16 & 0xff);
            v[3] = (byte)(c >> 24 & 0xff);
            value = new ReadOnlySequence<byte>(v);
        }
        public int Constructor => unchecked((int)0x997275b5);

        public ReadOnlySequence<byte> TLBytes => value;

        public bool IsMethod => throw new NotImplementedException();

        public static BoolTrue Read(int constructor, ref SequenceReader buff)
        {
            return new BoolTrue();
        }

        public ITLObject Execute(TLExecutionContext ctx)
        {
            throw new NotImplementedException();
        }

        public void Parse(ref SequenceReader buff)
        {
            
        }

        public void WriteTo(Span<byte> buff)
        {
            value.CopyTo(buff);
        }
    }
}

