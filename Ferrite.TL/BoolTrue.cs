/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
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

