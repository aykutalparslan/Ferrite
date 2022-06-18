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
public class SearchGlobal : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public SearchGlobal(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 1271290010;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            if (_flags[0])
            {
                writer.WriteInt32(_folderId, true);
            }

            writer.WriteTLString(_q);
            writer.Write(_filter.TLBytes, false);
            writer.WriteInt32(_minDate, true);
            writer.WriteInt32(_maxDate, true);
            writer.WriteInt32(_offsetRate, true);
            writer.Write(_offsetPeer.TLBytes, false);
            writer.WriteInt32(_offsetId, true);
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

    private int _folderId;
    public int FolderId
    {
        get => _folderId;
        set
        {
            serialized = false;
            _flags[0] = true;
            _folderId = value;
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

    private MessagesFilter _filter;
    public MessagesFilter Filter
    {
        get => _filter;
        set
        {
            serialized = false;
            _filter = value;
        }
    }

    private int _minDate;
    public int MinDate
    {
        get => _minDate;
        set
        {
            serialized = false;
            _minDate = value;
        }
    }

    private int _maxDate;
    public int MaxDate
    {
        get => _maxDate;
        set
        {
            serialized = false;
            _maxDate = value;
        }
    }

    private int _offsetRate;
    public int OffsetRate
    {
        get => _offsetRate;
        set
        {
            serialized = false;
            _offsetRate = value;
        }
    }

    private InputPeer _offsetPeer;
    public InputPeer OffsetPeer
    {
        get => _offsetPeer;
        set
        {
            serialized = false;
            _offsetPeer = value;
        }
    }

    private int _offsetId;
    public int OffsetId
    {
        get => _offsetId;
        set
        {
            serialized = false;
            _offsetId = value;
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
        if (_flags[0])
        {
            _folderId = buff.ReadInt32(true);
        }

        _q = buff.ReadTLString();
        _filter = (MessagesFilter)factory.Read(buff.ReadInt32(true), ref buff);
        _minDate = buff.ReadInt32(true);
        _maxDate = buff.ReadInt32(true);
        _offsetRate = buff.ReadInt32(true);
        _offsetPeer = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        _offsetId = buff.ReadInt32(true);
        _limit = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}