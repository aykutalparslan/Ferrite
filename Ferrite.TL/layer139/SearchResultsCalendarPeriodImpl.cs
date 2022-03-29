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

namespace Ferrite.TL.layer139;
public class SearchResultsCalendarPeriodImpl : SearchResultsCalendarPeriod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public SearchResultsCalendarPeriodImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -911191137;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(_date, true);
            writer.WriteInt32(_minMsgId, true);
            writer.WriteInt32(_maxMsgId, true);
            writer.WriteInt32(_count, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private int _date;
    public int Date
    {
        get => _date;
        set
        {
            serialized = false;
            _date = value;
        }
    }

    private int _minMsgId;
    public int MinMsgId
    {
        get => _minMsgId;
        set
        {
            serialized = false;
            _minMsgId = value;
        }
    }

    private int _maxMsgId;
    public int MaxMsgId
    {
        get => _maxMsgId;
        set
        {
            serialized = false;
            _maxMsgId = value;
        }
    }

    private int _count;
    public int Count
    {
        get => _count;
        set
        {
            serialized = false;
            _count = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _date = buff.ReadInt32(true);
        _minMsgId = buff.ReadInt32(true);
        _maxMsgId = buff.ReadInt32(true);
        _count = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}