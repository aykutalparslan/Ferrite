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

namespace Ferrite.TL.currentLayer.messages;
public class StickerSetImpl : StickerSet
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public StickerSetImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1240849242;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_set.TLBytes, false);
            writer.Write(_packs.TLBytes, false);
            writer.Write(_documents.TLBytes, false);
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

    private Vector<StickerPack> _packs;
    public Vector<StickerPack> Packs
    {
        get => _packs;
        set
        {
            serialized = false;
            _packs = value;
        }
    }

    private Vector<Document> _documents;
    public Vector<Document> Documents
    {
        get => _documents;
        set
        {
            serialized = false;
            _documents = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _set = (StickerSet)factory.Read(buff.ReadInt32(true), ref buff);
        buff.Skip(4); _packs  =  factory . Read < Vector < StickerPack > > ( ref  buff ) ; 
        buff.Skip(4); _documents  =  factory . Read < Vector < Document > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}