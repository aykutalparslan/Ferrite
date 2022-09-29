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
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL.mtproto;
using Ferrite.TL.ObjectMapper;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.contacts;
public class Search : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IContactsService _contacts;
    private readonly IMapperContext _mapper;
    private bool serialized = false;
    public Search(ITLObjectFactory objectFactory, IContactsService contacts,
        IMapperContext mapper)
    {
        factory = objectFactory;
        _contacts = contacts;
        _mapper = mapper;
    }

    public int Constructor => 301470424;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_q);
            writer.WriteInt32(_limit, true);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private string _q;
    public string Q
    {
        get => _q;
        set
        {
            serialized = false;
            _q = value;
        }
    }

    private int _limit;

    public int Limit
    {
        get => _limit;
        set
        {
            serialized = false;
            _limit = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var serviceResult = await _contacts.Search(ctx.CurrentAuthKeyId,
            _q, _limit);
        
        var found = factory.Resolve<FoundImpl>();
        found.Chats = factory.Resolve<Vector<Chat>>();
        found.Users = factory.Resolve<Vector<User>>();
        found.Results = factory.Resolve<Vector<Peer>>();
        found.MyResults = factory.Resolve<Vector<Peer>>();
        foreach (var c in serviceResult.Chats)
        {
            found.Chats.Add(_mapper.MapToTLObject<Chat, ChatDTO>(c));
        }
        foreach (var u in serviceResult.Users)
        {
            found.Users.Add(_mapper.MapToTLObject<User, UserDTO>(u));
        }
        foreach (var p in serviceResult.Results)
        {
            found.Results.Add(_mapper.MapToTLObject<Peer, PeerDTO>(p));
        }
        foreach (var p in serviceResult.MyResults)
        {
            found.MyResults.Add(_mapper.MapToTLObject<Peer, PeerDTO>(p));
        }
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        result.Result = found;
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _q = buff.ReadTLString();
        _limit = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}