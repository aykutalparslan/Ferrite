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
public class ForwardMessages : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public ForwardMessages(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -869258997;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_fromPeer.TLBytes, false);
            writer.Write(_id.TLBytes, false);
            writer.Write(_randomId.TLBytes, false);
            writer.Write(_toPeer.TLBytes, false);
            if (_flags[10])
            {
                writer.WriteInt32(_scheduleDate, true);
            }

            if (_flags[13])
            {
                writer.Write(_sendAs.TLBytes, false);
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

    public bool Silent
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
        }
    }

    public bool Background
    {
        get => _flags[6];
        set
        {
            serialized = false;
            _flags[6] = value;
        }
    }

    public bool WithMyScore
    {
        get => _flags[8];
        set
        {
            serialized = false;
            _flags[8] = value;
        }
    }

    public bool DropAuthor
    {
        get => _flags[11];
        set
        {
            serialized = false;
            _flags[11] = value;
        }
    }

    public bool DropMediaCaptions
    {
        get => _flags[12];
        set
        {
            serialized = false;
            _flags[12] = value;
        }
    }

    public bool Noforwards
    {
        get => _flags[14];
        set
        {
            serialized = false;
            _flags[14] = value;
        }
    }

    private InputPeer _fromPeer;
    public InputPeer FromPeer
    {
        get => _fromPeer;
        set
        {
            serialized = false;
            _fromPeer = value;
        }
    }

    private VectorOfInt _id;
    public VectorOfInt Id
    {
        get => _id;
        set
        {
            serialized = false;
            _id = value;
        }
    }

    private VectorOfLong _randomId;
    public VectorOfLong RandomId
    {
        get => _randomId;
        set
        {
            serialized = false;
            _randomId = value;
        }
    }

    private InputPeer _toPeer;
    public InputPeer ToPeer
    {
        get => _toPeer;
        set
        {
            serialized = false;
            _toPeer = value;
        }
    }

    private int _scheduleDate;
    public int ScheduleDate
    {
        get => _scheduleDate;
        set
        {
            serialized = false;
            _flags[10] = true;
            _scheduleDate = value;
        }
    }

    private InputPeer _sendAs;
    public InputPeer SendAs
    {
        get => _sendAs;
        set
        {
            serialized = false;
            _flags[13] = true;
            _sendAs = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _fromPeer = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        buff.Skip(4); _id  =  factory . Read < VectorOfInt > ( ref  buff ) ; 
        buff.Skip(4); _randomId  =  factory . Read < VectorOfLong > ( ref  buff ) ; 
        _toPeer = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[10])
        {
            _scheduleDate = buff.ReadInt32(true);
        }

        if (_flags[13])
        {
            _sendAs = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        }
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}