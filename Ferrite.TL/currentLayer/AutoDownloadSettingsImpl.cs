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
public class AutoDownloadSettingsImpl : AutoDownloadSettings
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public AutoDownloadSettingsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -532532493;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt32(_photoSizeMax, true);
            writer.WriteInt32(_videoSizeMax, true);
            writer.WriteInt32(_fileSizeMax, true);
            writer.WriteInt32(_videoUploadMaxbitrate, true);
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

    public bool Disabled
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool VideoPreloadLarge
    {
        get => _flags[1];
        set
        {
            serialized = false;
            _flags[1] = value;
        }
    }

    public bool AudioPreloadNext
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    public bool PhonecallsLessData
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
        }
    }

    private int _photoSizeMax;
    public int PhotoSizeMax
    {
        get => _photoSizeMax;
        set
        {
            serialized = false;
            _photoSizeMax = value;
        }
    }

    private int _videoSizeMax;
    public int VideoSizeMax
    {
        get => _videoSizeMax;
        set
        {
            serialized = false;
            _videoSizeMax = value;
        }
    }

    private int _fileSizeMax;
    public int FileSizeMax
    {
        get => _fileSizeMax;
        set
        {
            serialized = false;
            _fileSizeMax = value;
        }
    }

    private int _videoUploadMaxbitrate;
    public int VideoUploadMaxbitrate
    {
        get => _videoUploadMaxbitrate;
        set
        {
            serialized = false;
            _videoUploadMaxbitrate = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _photoSizeMax = buff.ReadInt32(true);
        _videoSizeMax = buff.ReadInt32(true);
        _fileSizeMax = buff.ReadInt32(true);
        _videoUploadMaxbitrate = buff.ReadInt32(true);
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}