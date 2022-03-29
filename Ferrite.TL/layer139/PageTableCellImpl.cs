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
public class PageTableCellImpl : PageTableCell
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PageTableCellImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 878078826;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            if (_flags[7])
            {
                writer.Write(_text.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.WriteInt32(_colspan, true);
            }

            if (_flags[2])
            {
                writer.WriteInt32(_rowspan, true);
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

    public bool Header
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool AlignCenter
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
        }
    }

    public bool AlignRight
    {
        get => _flags[4];
        set
        {
            serialized = false;
            _flags[4] = value;
        }
    }

    public bool ValignMiddle
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
        }
    }

    public bool ValignBottom
    {
        get => _flags[6];
        set
        {
            serialized = false;
            _flags[6] = value;
        }
    }

    private RichText _text;
    public RichText Text
    {
        get => _text;
        set
        {
            serialized = false;
            _flags[7] = true;
            _text = value;
        }
    }

    private int _colspan;
    public int Colspan
    {
        get => _colspan;
        set
        {
            serialized = false;
            _flags[1] = true;
            _colspan = value;
        }
    }

    private int _rowspan;
    public int Rowspan
    {
        get => _rowspan;
        set
        {
            serialized = false;
            _flags[2] = true;
            _rowspan = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        if (_flags[7])
        {
            buff.Skip(4);
            _text = factory.Read<RichText>(ref buff);
        }

        if (_flags[1])
        {
            _colspan = buff.ReadInt32(true);
        }

        if (_flags[2])
        {
            _rowspan = buff.ReadInt32(true);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}