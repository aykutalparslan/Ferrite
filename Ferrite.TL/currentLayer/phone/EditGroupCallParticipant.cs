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

namespace Ferrite.TL.currentLayer.phone;
public class EditGroupCallParticipant : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public EditGroupCallParticipant(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -1524155713;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_call.TLBytes, false);
            writer.Write(_participant.TLBytes, false);
            if (_flags[0])
            {
                writer.WriteInt32(Bool.GetConstructor(_muted), true);
            }

            if (_flags[1])
            {
                writer.WriteInt32(_volume, true);
            }

            if (_flags[2])
            {
                writer.WriteInt32(Bool.GetConstructor(_raiseHand), true);
            }

            if (_flags[3])
            {
                writer.WriteInt32(Bool.GetConstructor(_videoStopped), true);
            }

            if (_flags[4])
            {
                writer.WriteInt32(Bool.GetConstructor(_videoPaused), true);
            }

            if (_flags[5])
            {
                writer.WriteInt32(Bool.GetConstructor(_presentationPaused), true);
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

    private InputGroupCall _call;
    public InputGroupCall Call
    {
        get => _call;
        set
        {
            serialized = false;
            _call = value;
        }
    }

    private InputPeer _participant;
    public InputPeer Participant
    {
        get => _participant;
        set
        {
            serialized = false;
            _participant = value;
        }
    }

    private bool _muted;
    public bool Muted
    {
        get => _muted;
        set
        {
            serialized = false;
            _flags[0] = true;
            _muted = value;
        }
    }

    private int _volume;
    public int Volume
    {
        get => _volume;
        set
        {
            serialized = false;
            _flags[1] = true;
            _volume = value;
        }
    }

    private bool _raiseHand;
    public bool RaiseHand
    {
        get => _raiseHand;
        set
        {
            serialized = false;
            _flags[2] = true;
            _raiseHand = value;
        }
    }

    private bool _videoStopped;
    public bool VideoStopped
    {
        get => _videoStopped;
        set
        {
            serialized = false;
            _flags[3] = true;
            _videoStopped = value;
        }
    }

    private bool _videoPaused;
    public bool VideoPaused
    {
        get => _videoPaused;
        set
        {
            serialized = false;
            _flags[4] = true;
            _videoPaused = value;
        }
    }

    private bool _presentationPaused;
    public bool PresentationPaused
    {
        get => _presentationPaused;
        set
        {
            serialized = false;
            _flags[5] = true;
            _presentationPaused = value;
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
        _call = (InputGroupCall)factory.Read(buff.ReadInt32(true), ref buff);
        _participant = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[0])
        {
            _muted = Bool.Read(ref buff);
        }

        if (_flags[1])
        {
            _volume = buff.ReadInt32(true);
        }

        if (_flags[2])
        {
            _raiseHand = Bool.Read(ref buff);
        }

        if (_flags[3])
        {
            _videoStopped = Bool.Read(ref buff);
        }

        if (_flags[4])
        {
            _videoPaused = Bool.Read(ref buff);
        }

        if (_flags[5])
        {
            _presentationPaused = Bool.Read(ref buff);
        }
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}