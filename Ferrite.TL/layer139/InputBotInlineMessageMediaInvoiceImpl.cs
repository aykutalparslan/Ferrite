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
public class InputBotInlineMessageMediaInvoiceImpl : InputBotInlineMessage
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public InputBotInlineMessageMediaInvoiceImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -672693723;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteTLString(_title);
            writer.WriteTLString(_description);
            if (_flags[0])
            {
                writer.Write(_photo.TLBytes, false);
            }

            writer.Write(_invoice.TLBytes, false);
            writer.WriteTLBytes(_payload);
            writer.WriteTLString(_provider);
            writer.Write(_providerData.TLBytes, false);
            if (_flags[2])
            {
                writer.Write(_replyMarkup.TLBytes, false);
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

    private InputWebDocument _photo;
    public InputWebDocument Photo
    {
        get => _photo;
        set
        {
            serialized = false;
            _flags[0] = true;
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

    private string _provider;
    public string Provider
    {
        get => _provider;
        set
        {
            serialized = false;
            _provider = value;
        }
    }

    private DataJSON _providerData;
    public DataJSON ProviderData
    {
        get => _providerData;
        set
        {
            serialized = false;
            _providerData = value;
        }
    }

    private ReplyMarkup _replyMarkup;
    public ReplyMarkup ReplyMarkup
    {
        get => _replyMarkup;
        set
        {
            serialized = false;
            _flags[2] = true;
            _replyMarkup = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _title = buff.ReadTLString();
        _description = buff.ReadTLString();
        if (_flags[0])
        {
            _photo = (InputWebDocument)factory.Read(buff.ReadInt32(true), ref buff);
        }

        _invoice = (Invoice)factory.Read(buff.ReadInt32(true), ref buff);
        _payload = buff.ReadTLBytes().ToArray();
        _provider = buff.ReadTLString();
        _providerData = (DataJSON)factory.Read(buff.ReadInt32(true), ref buff);
        if (_flags[2])
        {
            _replyMarkup = (ReplyMarkup)factory.Read(buff.ReadInt32(true), ref buff);
        }
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}