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
public class GroupCallParticipantVideoImpl : GroupCallParticipantVideo
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public GroupCallParticipantVideoImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1735736008;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteTLString(_endpoint);
            writer.Write(_sourceGroups.TLBytes, false);
            if (_flags[1])
            {
                writer.WriteInt32(_audioSource, true);
            }

            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private Flags _flags;
    public Flags Flags
    {
        get => _flags;
        set
        {
            serialized = false;
            _flags = value;
        }
    }

    public bool Paused
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    private string _endpoint;
    public string Endpoint
    {
        get => _endpoint;
        set
        {
            serialized = false;
            _endpoint = value;
        }
    }

    private Vector<GroupCallParticipantVideoSourceGroup> _sourceGroups;
    public Vector<GroupCallParticipantVideoSourceGroup> SourceGroups
    {
        get => _sourceGroups;
        set
        {
            serialized = false;
            _sourceGroups = value;
        }
    }

    private int _audioSource;
    public int AudioSource
    {
        get => _audioSource;
        set
        {
            serialized = false;
            _flags[1] = true;
            _audioSource = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _endpoint = buff.ReadTLString();
        buff.Skip(4); _sourceGroups  =  factory . Read < Vector < GroupCallParticipantVideoSourceGroup > > ( ref  buff ) ; 
        if (_flags[1])
        {
            _audioSource = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}