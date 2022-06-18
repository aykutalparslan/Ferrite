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
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer;
public class StickerSetMultiCoveredImpl : StickerSetCovered
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public StickerSetMultiCoveredImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 872932635;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_set.TLBytes, false);
            writer.Write(_covers.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private StickerSet _set;
    public StickerSet Set
    {
        get => _set;
        set
        {
            serialized = false;
            _set = value;
        }
    }

    private Vector<Document> _covers;
    public Vector<Document> Covers
    {
        get => _covers;
        set
        {
            serialized = false;
            _covers = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _set = (StickerSet)factory.Read(buff.ReadInt32(true), ref buff);
        buff.Skip(4); _covers  =  factory . Read < Vector < Document > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}