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

namespace Ferrite.TL.currentLayer.channels;
public class GetAdminLog : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public GetAdminLog(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 870184064;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.Write(_channel.TLBytes, false);
            writer.WriteTLString(_q);
            if (_flags[0])
            {
                writer.Write(_eventsFilter.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.Write(_admins.TLBytes, false);
            }

            writer.WriteInt64(_maxId, true);
            writer.WriteInt64(_minId, true);
            writer.WriteInt32(_limit, true);
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

    private InputChannel _channel;
    public InputChannel Channel
    {
        get => _channel;
        set
        {
            serialized = false;
            _channel = value;
        }
    }

    private string _q;
    public string Q
    {
        get => _q;
        set
        {
            serialized = false;
            _q = value;
        }
    }

    private ChannelAdminLogEventsFilter _eventsFilter;
    public ChannelAdminLogEventsFilter EventsFilter
    {
        get => _eventsFilter;
        set
        {
            serialized = false;
            _flags[0] = true;
            _eventsFilter = value;
        }
    }

    private Vector<InputUser> _admins;
    public Vector<InputUser> Admins
    {
        get => _admins;
        set
        {
            serialized = false;
            _flags[1] = true;
            _admins = value;
        }
    }

    private long _maxId;
    public long MaxId
    {
        get => _maxId;
        set
        {
            serialized = false;
            _maxId = value;
        }
    }

    private long _minId;
    public long MinId
    {
        get => _minId;
        set
        {
            serialized = false;
            _minId = value;
        }
    }

    private int _limit;
    public int Limit
    {
        get => _limit;
        set
        {
            serialized = false;
            _limit = value;
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
        _channel = (InputChannel)factory.Read(buff.ReadInt32(true), ref buff);
        _q = buff.ReadTLString();
        if (_flags[0])
        {
            _eventsFilter = (ChannelAdminLogEventsFilter)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[1])
        {
            buff.Skip(4);
            _admins = factory.Read<Vector<InputUser>>(ref buff);
        }

        _maxId = buff.ReadInt64(true);
        _minId = buff.ReadInt64(true);
        _limit = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}