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
public class MessageActionPaymentSentMeImpl : MessageAction
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public MessageActionPaymentSentMeImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1892568281;
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
            writer.WriteInt64(_totalAmount, true);
            writer.WriteTLBytes(_payload);
            if (_flags[0])
            {
                writer.Write(_info.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.WriteTLString(_shippingOptionId);
            }

            writer.Write(_charge.TLBytes, false);
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

    private long _totalAmount;
    public long TotalAmount
    {
        get => _totalAmount;
        set
        {
            serialized = false;
            _totalAmount = value;
        }
    }

    private byte[] _payload;
    public byte[] Payload
    {
        get => _payload;
        set
        {
            serialized = false;
            _payload = value;
        }
    }

    private PaymentRequestedInfo _info;
    public PaymentRequestedInfo Info
    {
        get => _info;
        set
        {
            serialized = false;
            _flags[0] = true;
            _info = value;
        }
    }

    private string _shippingOptionId;
    public string ShippingOptionId
    {
        get => _shippingOptionId;
        set
        {
            serialized = false;
            _flags[1] = true;
            _shippingOptionId = value;
        }
    }

    private PaymentCharge _charge;
    public PaymentCharge Charge
    {
        get => _charge;
        set
        {
            serialized = false;
            _charge = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _currency = buff.ReadTLString();
        _totalAmount = buff.ReadInt64(true);
        _payload = buff.ReadTLBytes().ToArray();
        if (_flags[0])
        {
            buff.Skip(4);
            _info = factory.Read<PaymentRequestedInfo>(ref buff);
        }

        if (_flags[1])
        {
            _shippingOptionId = buff.ReadTLString();
        }

        buff.Skip(4); _charge  =  factory . Read < PaymentCharge > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}