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
public class SendPaymentForm : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public SendPaymentForm(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 818134173;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt64(_formId, true);
            writer.Write(_peer.TLBytes, false);
            writer.WriteInt32(_msgId, true);
            if (_flags[0])
            {
                writer.WriteTLString(_requestedInfoId);
            }

            if (_flags[1])
            {
                writer.WriteTLString(_shippingOptionId);
            }

            writer.Write(_credentials.TLBytes, false);
            if (_flags[2])
            {
                writer.WriteInt64(_tipAmount, true);
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

    private long _formId;
    public long FormId
    {
        get => _formId;
        set
        {
            serialized = false;
            _formId = value;
        }
    }

    private InputPeer _peer;
    public InputPeer Peer
    {
        get => _peer;
        set
        {
            serialized = false;
            _peer = value;
        }
    }

    private int _msgId;
    public int MsgId
    {
        get => _msgId;
        set
        {
            serialized = false;
            _msgId = value;
        }
    }

    private string _requestedInfoId;
    public string RequestedInfoId
    {
        get => _requestedInfoId;
        set
        {
            serialized = false;
            _flags[0] = true;
            _requestedInfoId = value;
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

    private InputPaymentCredentials _credentials;
    public InputPaymentCredentials Credentials
    {
        get => _credentials;
        set
        {
            serialized = false;
            _credentials = value;
        }
    }

    private long _tipAmount;
    public long TipAmount
    {
        get => _tipAmount;
        set
        {
            serialized = false;
            _flags[2] = true;
            _tipAmount = value;
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
        _formId = buff.ReadInt64(true);
        buff.Skip(4); _peer  =  factory . Read < InputPeer > ( ref  buff ) ; 
        _msgId = buff.ReadInt32(true);
        if (_flags[0])
        {
            _requestedInfoId = buff.ReadTLString();
        }

        if (_flags[1])
        {
            _shippingOptionId = buff.ReadTLString();
        }

        buff.Skip(4); _credentials  =  factory . Read < InputPaymentCredentials > ( ref  buff ) ; 
        if (_flags[2])
        {
            _tipAmount = buff.ReadInt64(true);
        }
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}