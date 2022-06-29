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
public class NotificationSoundLocalImpl : NotificationSound
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public NotificationSoundLocalImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }
    
    private string _title;
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            serialized = false;
        }
    }
    private string _data;
    public string Data
    {
        get => _data;
        set
        {
            _data = value;
            serialized = false;
        }
    }

    public override int Constructor => unchecked((int)0x830b9ae4);
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_title);
            writer.WriteTLString(_data);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _title = buff.ReadTLString();
        _data = buff.ReadTLString();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}