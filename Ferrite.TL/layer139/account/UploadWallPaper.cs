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
public class UploadWallPaper : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public UploadWallPaper(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -578472351;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_file.TLBytes, false);
            writer.WriteTLString(_mimeType);
            writer.Write(_settings.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private InputFile _file;
    public InputFile File
    {
        get => _file;
        set
        {
            serialized = false;
            _file = value;
        }
    }

    private string _mimeType;
    public string MimeType
    {
        get => _mimeType;
        set
        {
            serialized = false;
            _mimeType = value;
        }
    }

    private WallPaperSettings _settings;
    public WallPaperSettings Settings
    {
        get => _settings;
        set
        {
            serialized = false;
            _settings = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        buff.Skip(4); _file  =  factory . Read < InputFile > ( ref  buff ) ; 
        _mimeType = buff.ReadTLString();
        buff.Skip(4); _settings  =  factory . Read < WallPaperSettings > ( ref  buff ) ; 
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}