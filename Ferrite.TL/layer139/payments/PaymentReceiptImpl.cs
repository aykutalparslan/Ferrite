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

namespace Ferrite.TL.layer139.payments;
public class PaymentReceiptImpl : PaymentReceipt
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PaymentReceiptImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1891958275;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt32(_date, true);
            writer.WriteInt64(_botId, true);
            writer.WriteInt64(_providerId, true);
            writer.WriteTLString(_title);
            writer.WriteTLString(_description);
            if (_flags[2])
            {
                writer.Write(_photo.TLBytes, false);
            }

            writer.Write(_invoice.TLBytes, false);
            if (_flags[0])
            {
                writer.Write(_info.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.Write(_shipping.TLBytes, false);
            }

            if (_flags[3])
            {
                writer.WriteInt64(_tipAmount, true);
            }

            writer.WriteTLString(_currency);
            writer.WriteInt64(_totalAmount, true);
            writer.WriteTLString(_credentialsTitle);
            writer.Write(_users.TLBytes, false);
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

    private long _botId;
    public long BotId
    {
        get => _botId;
        set
        {
            serialized = false;
            _botId = value;
        }
    }

    private long _providerId;
    public long ProviderId
    {
        get => _providerId;
        set
        {
            serialized = false;
            _providerId = value;
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

    private string _description;
    public string Description
    {
        get => _description;
        set
        {
            serialized = false;
            _description = value;
        }
    }

    private WebDocument _photo;
    public WebDocument Photo
    {
        get => _photo;
        set
        {
            serialized = false;
            _flags[2] = true;
            _photo = value;
        }
    }

    private Invoice _invoice;
    public Invoice Invoice
    {
        get => _invoice;
        set
        {
            serialized = false;
            _invoice = value;
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

    private ShippingOption _shipping;
    public ShippingOption Shipping
    {
        get => _shipping;
        set
        {
            serialized = false;
            _flags[1] = true;
            _shipping = value;
        }
    }

    private long _tipAmount;
    public long TipAmount
    {
        get => _tipAmount;
        set
        {
            serialized = false;
            _flags[3] = true;
            _tipAmount = value;
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

    private string _credentialsTitle;
    public string CredentialsTitle
    {
        get => _credentialsTitle;
        set
        {
            serialized = false;
            _credentialsTitle = value;
        }
    }

    private Vector<User> _users;
    public Vector<User> Users
    {
        get => _users;
        set
        {
            serialized = false;
            _users = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _date = buff.ReadInt32(true);
        _botId = buff.ReadInt64(true);
        _providerId = buff.ReadInt64(true);
        _title = buff.ReadTLString();
        _description = buff.ReadTLString();
        if (_flags[2])
        {
            _photo = (WebDocument)factory.Read(buff.ReadInt32(true), ref buff);
        }

        _invoice = (Invoice)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[0])
        {
            _info = (PaymentRequestedInfo)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[1])
        {
            _shipping = (ShippingOption)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[3])
        {
            _tipAmount = buff.ReadInt64(true);
        }

        _currency = buff.ReadTLString();
        _totalAmount = buff.ReadInt64(true);
        _credentialsTitle = buff.ReadTLString();
        buff.Skip(4); _users  =  factory . Read < Vector < User > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}