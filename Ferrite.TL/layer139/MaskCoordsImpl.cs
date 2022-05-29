/*
 *   Project Ferrite is an Implementation of the Telegram Server API
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
public class MaskCoordsImpl : MaskCoords
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public MaskCoordsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1361650766;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt32(_n, true);
            writer.Write(BitConverter.GetBytes(_x));
            writer.Write(BitConverter.GetBytes(_y));
            writer.Write(BitConverter.GetBytes(_zoom));
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private int _n;
    public int N
    {
        get => _n;
        set
        {
            serialized = false;
            _n = value;
        }
    }

    private double _x;
    public double X
    {
        get => _x;
        set
        {
            serialized = false;
            _x = value;
        }
    }

    private double _y;
    public double Y
    {
        get => _y;
        set
        {
            serialized = false;
            _y = value;
        }
    }

    private double _zoom;
    public double Zoom
    {
        get => _zoom;
        set
        {
            serialized = false;
            _zoom = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _n = buff.ReadInt32(true);
        _x = buff.Read<double>();
        _y = buff.Read<double>();
        _zoom = buff.Read<double>();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}