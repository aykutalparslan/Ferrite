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

namespace Ferrite.TL.currentLayer.photos;
public class PhotoImpl : Photo
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PhotoImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 539045032;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_photo.TLBytes, false);
            writer.Write(_users.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private currentLayer.Photo _photo;
    public currentLayer.Photo Photo
    {
        get => _photo;
        set
        {
            serialized = false;
            _photo = value;
        }
    }

    private Vector<User> _users;
    public Vector<User> Users
    {
        get => _users;
        set
        {
            serialized = false;
            _users = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _photo = (currentLayer.Photo)factory.Read(buff.ReadInt32(true), ref buff);
        buff.Skip(4); _users  =  factory . Read < Vector < User > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}