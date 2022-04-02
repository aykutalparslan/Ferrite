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

namespace Ferrite.TL.layer139.channels;
public class EditAdmin : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public EditAdmin(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -751007486;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_channel.TLBytes, false);
            writer.Write(_userId.TLBytes, false);
            writer.Write(_adminRights.TLBytes, false);
            writer.WriteTLString(_rank);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private InputChannel _channel;
    public InputChannel Channel
    {
        get => _channel;
        set
        {
            serialized = false;
            _channel = value;
        }
    }

    private InputUser _userId;
    public InputUser UserId
    {
        get => _userId;
        set
        {
            serialized = false;
            _userId = value;
        }
    }

    private ChatAdminRights _adminRights;
    public ChatAdminRights AdminRights
    {
        get => _adminRights;
        set
        {
            serialized = false;
            _adminRights = value;
        }
    }

    private string _rank;
    public string Rank
    {
        get => _rank;
        set
        {
            serialized = false;
            _rank = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _channel = (InputChannel)factory.Read(buff.ReadInt32(true), ref buff);
        _userId = (InputUser)factory.Read(buff.ReadInt32(true), ref buff);
        _adminRights = (ChatAdminRights)factory.Read(buff.ReadInt32(true), ref buff);
        _rank = buff.ReadTLString();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}