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

namespace Ferrite.TL.currentLayer.payments;
public class PaymentFormImpl : PaymentForm
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public PaymentFormImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 378828315;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt64(_formId, true);
            writer.WriteInt64(_botId, true);
            writer.Write(_invoice.TLBytes, false);
            writer.WriteInt64(_providerId, true);
            writer.WriteTLString(_url);
            if (_flags[4])
            {
                writer.WriteTLString(_nativeProvider);
            }

            if (_flags[4])
            {
                writer.Write(_nativeParams.TLBytes, false);
            }

            if (_flags[0])
            {
                writer.Write(_savedInfo.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.Write(_savedCredentials.TLBytes, false);
            }

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

    public bool CanSaveCredentials
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    public bool PasswordMissing
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
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

    private string _url;
    public string Url
    {
        get => _url;
        set
        {
            serialized = false;
            _url = value;
        }
    }

    private string _nativeProvider;
    public string NativeProvider
    {
        get => _nativeProvider;
        set
        {
            serialized = false;
            _flags[4] = true;
            _nativeProvider = value;
        }
    }

    private DataJSON _nativeParams;
    public DataJSON NativeParams
    {
        get => _nativeParams;
        set
        {
            serialized = false;
            _flags[4] = true;
            _nativeParams = value;
        }
    }

    private PaymentRequestedInfo _savedInfo;
    public PaymentRequestedInfo SavedInfo
    {
        get => _savedInfo;
        set
        {
            serialized = false;
            _flags[0] = true;
            _savedInfo = value;
        }
    }

    private PaymentSavedCredentials _savedCredentials;
    public PaymentSavedCredentials SavedCredentials
    {
        get => _savedCredentials;
        set
        {
            serialized = false;
            _flags[1] = true;
            _savedCredentials = value;
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
        _formId = buff.ReadInt64(true);
        _botId = buff.ReadInt64(true);
        _invoice = (Invoice)factory.Read(buff.ReadInt32(true), ref buff);
        _providerId = buff.ReadInt64(true);
        _url = buff.ReadTLString();
        if (_flags[4])
        {
            _nativeProvider = buff.ReadTLString();
        }

        if (_flags[4])
        {
            _nativeParams = (DataJSON)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[0])
        {
            _savedInfo = (PaymentRequestedInfo)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[1])
        {
            _savedCredentials = (PaymentSavedCredentials)factory.Read(buff.ReadInt32(true), ref buff);
        }

        buff.Skip(4); _users  =  factory . Read < Vector < User > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}