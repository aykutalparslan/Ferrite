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

namespace Ferrite.TL.layer139.account;
public class GetTheme : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public GetTheme(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -1919060949;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_format);
            writer.Write(_theme.TLBytes, false);
            writer.WriteInt64(_documentId, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private string _format;
    public string Format
    {
        get => _format;
        set
        {
            serialized = false;
            _format = value;
        }
    }

    private InputTheme _theme;
    public InputTheme Theme
    {
        get => _theme;
        set
        {
            serialized = false;
            _theme = value;
        }
    }

    private long _documentId;
    public long DocumentId
    {
        get => _documentId;
        set
        {
            serialized = false;
            _documentId = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _format = buff.ReadTLString();
        _theme = (InputTheme)factory.Read(buff.ReadInt32(true), ref buff);
        _documentId = buff.ReadInt64(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}