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
public class DialogFilterImpl : DialogFilter
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public DialogFilterImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1949890536;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt32(_id, true);
            writer.WriteTLString(_title);
            if (_flags[25])
            {
                writer.WriteTLString(_emoticon);
            }

            writer.Write(_pinnedPeers.TLBytes, false);
            writer.Write(_includePeers.TLBytes, false);
            writer.Write(_excludePeers.TLBytes, false);
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

    public bool Contacts
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool NonContacts
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool Groups
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    public bool Broadcasts
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
        }
    }

    public bool Bots
    {
        get => _flags[4];
        set
        {
            serialized = false;
            _flags[4] = value;
        }
    }

    public bool ExcludeMuted
    {
        get => _flags[11];
        set
        {
            serialized = false;
            _flags[11] = value;
        }
    }

    public bool ExcludeRead
    {
        get => _flags[12];
        set
        {
            serialized = false;
            _flags[12] = value;
        }
    }

    public bool ExcludeArchived
    {
        get => _flags[13];
        set
        {
            serialized = false;
            _flags[13] = value;
        }
    }

    private int _id;
    public int Id
    {
        get => _id;
        set
        {
            serialized = false;
            _id = value;
        }
    }

    private string _title;
    public string Title
    {
        get => _title;
        set
        {
            serialized = false;
            _title = value;
        }
    }

    private string _emoticon;
    public string Emoticon
    {
        get => _emoticon;
        set
        {
            serialized = false;
            _flags[25] = true;
            _emoticon = value;
        }
    }

    private Vector<InputPeer> _pinnedPeers;
    public Vector<InputPeer> PinnedPeers
    {
        get => _pinnedPeers;
        set
        {
            serialized = false;
            _pinnedPeers = value;
        }
    }

    private Vector<InputPeer> _includePeers;
    public Vector<InputPeer> IncludePeers
    {
        get => _includePeers;
        set
        {
            serialized = false;
            _includePeers = value;
        }
    }

    private Vector<InputPeer> _excludePeers;
    public Vector<InputPeer> ExcludePeers
    {
        get => _excludePeers;
        set
        {
            serialized = false;
            _excludePeers = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _id = buff.ReadInt32(true);
        _title = buff.ReadTLString();
        if (_flags[25])
        {
            _emoticon = buff.ReadTLString();
        }

        buff.Skip(4); _pinnedPeers  =  factory . Read < Vector < InputPeer > > ( ref  buff ) ; 
        buff.Skip(4); _includePeers  =  factory . Read < Vector < InputPeer > > ( ref  buff ) ; 
        buff.Skip(4); _excludePeers  =  factory . Read < Vector < InputPeer > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}