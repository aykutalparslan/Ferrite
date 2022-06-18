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

namespace Ferrite.TL.currentLayer.account;
public class AutoDownloadSettingsImpl : AutoDownloadSettings
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public AutoDownloadSettingsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => 1674235686;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_low.TLBytes, false);
            writer.Write(_medium.TLBytes, false);
            writer.Write(_high.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private AutoDownloadSettings _low;
    public AutoDownloadSettings Low
    {
        get => _low;
        set
        {
            serialized = false;
            _low = value;
        }
    }

    private AutoDownloadSettings _medium;
    public AutoDownloadSettings Medium
    {
        get => _medium;
        set
        {
            serialized = false;
            _medium = value;
        }
    }

    private AutoDownloadSettings _high;
    public AutoDownloadSettings High
    {
        get => _high;
        set
        {
            serialized = false;
            _high = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _low = (AutoDownloadSettings)factory.Read(buff.ReadInt32(true), ref buff);
        _medium = (AutoDownloadSettings)factory.Read(buff.ReadInt32(true), ref buff);
        _high = (AutoDownloadSettings)factory.Read(buff.ReadInt32(true), ref buff);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}