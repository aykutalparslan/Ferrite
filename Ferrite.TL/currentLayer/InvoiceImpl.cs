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
public class InvoiceImpl : Invoice
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InvoiceImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 215516896;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteTLString(_currency);
            writer.Write(_prices.TLBytes, false);
            if (_flags[8])
            {
                writer.WriteInt64(_maxTipAmount, true);
            }

            if (_flags[8])
            {
                writer.Write(_suggestedTipAmounts.TLBytes, false);
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

    public bool Test
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool NameRequested
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool PhoneRequested
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    public bool EmailRequested
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
        }
    }

    public bool ShippingAddressRequested
    {
        get => _flags[4];
        set
        {
            serialized = false;
            _flags[4] = value;
        }
    }

    public bool Flexible
    {
        get => _flags[5];
        set
        {
            serialized = false;
            _flags[5] = value;
        }
    }

    public bool PhoneToProvider
    {
        get => _flags[6];
        set
        {
            serialized = false;
            _flags[6] = value;
        }
    }

    public bool EmailToProvider
    {
        get => _flags[7];
        set
        {
            serialized = false;
            _flags[7] = value;
        }
    }

    private string _currency;
    public string Currency
    {
        get => _currency;
        set
        {
            serialized = false;
            _currency = value;
        }
    }

    private Vector<LabeledPrice> _prices;
    public Vector<LabeledPrice> Prices
    {
        get => _prices;
        set
        {
            serialized = false;
            _prices = value;
        }
    }

    private long _maxTipAmount;
    public long MaxTipAmount
    {
        get => _maxTipAmount;
        set
        {
            serialized = false;
            _flags[8] = true;
            _maxTipAmount = value;
        }
    }

    private VectorOfLong _suggestedTipAmounts;
    public VectorOfLong SuggestedTipAmounts
    {
        get => _suggestedTipAmounts;
        set
        {
            serialized = false;
            _flags[8] = true;
            _suggestedTipAmounts = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _currency = buff.ReadTLString();
        buff.Skip(4); _prices  =  factory . Read < Vector < LabeledPrice > > ( ref  buff ) ; 
        if (_flags[8])
        {
            _maxTipAmount = buff.ReadInt64(true);
        }

        if (_flags[8])
        {
            buff.Skip(4);
            _suggestedTipAmounts = factory.Read<VectorOfLong>(ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}