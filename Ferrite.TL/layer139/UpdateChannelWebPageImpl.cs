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
public class UpdateChannelWebPageImpl : Update
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public UpdateChannelWebPageImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 791390623;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_channelId, true);
            writer.Write(_webpage.TLBytes, false);
            writer.WriteInt32(_pts, true);
            writer.WriteInt32(_ptsCount, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long _channelId;
    public long ChannelId
    {
        get => _channelId;
        set
        {
            serialized = false;
            _channelId = value;
        }
    }

    private WebPage _webpage;
    public WebPage Webpage
    {
        get => _webpage;
        set
        {
            serialized = false;
            _webpage = value;
        }
    }

    private int _pts;
    public int Pts
    {
        get => _pts;
        set
        {
            serialized = false;
            _pts = value;
        }
    }

    private int _ptsCount;
    public int PtsCount
    {
        get => _ptsCount;
        set
        {
            serialized = false;
            _ptsCount = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _channelId = buff.ReadInt64(true);
        buff.Skip(4); _webpage  =  factory . Read < WebPage > ( ref  buff ) ; 
        _pts = buff.ReadInt32(true);
        _ptsCount = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}