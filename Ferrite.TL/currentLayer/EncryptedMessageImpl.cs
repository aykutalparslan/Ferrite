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
public class EncryptedMessageImpl : EncryptedMessage
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public EncryptedMessageImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -317144808;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_randomId, true);
            writer.WriteInt32(_chatId, true);
            writer.WriteInt32(_date, true);
            writer.WriteTLBytes(_bytes);
            writer.Write(_file.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long _randomId;
    public long RandomId
    {
        get => _randomId;
        set
        {
            serialized = false;
            _randomId = value;
        }
    }

    private int _chatId;
    public int ChatId
    {
        get => _chatId;
        set
        {
            serialized = false;
            _chatId = value;
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

    private byte[] _bytes;
    public byte[] Bytes
    {
        get => _bytes;
        set
        {
            serialized = false;
            _bytes = value;
        }
    }

    private EncryptedFile _file;
    public EncryptedFile File
    {
        get => _file;
        set
        {
            serialized = false;
            _file = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _randomId = buff.ReadInt64(true);
        _chatId = buff.ReadInt32(true);
        _date = buff.ReadInt32(true);
        _bytes = buff.ReadTLBytes().ToArray();
        _file = (EncryptedFile)factory.Read(buff.ReadInt32(true), ref buff);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}