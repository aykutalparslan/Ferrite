// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using Ferrite.Data;
using Ferrite.Data.Contacts;
using Ferrite.TL.currentLayer;
using Ferrite.TL.currentLayer.contacts;

namespace Ferrite.TL.ObjectMapper;

public class ResolvedPeerMapper : ITLObjectMapper<ResolvedPeer, ResolvedPeerDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public ResolvedPeerMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public ResolvedPeerDTO MapToDTO(ResolvedPeer obj)
    {
        throw new NotImplementedException();
    }

    public ResolvedPeer MapToTLObject(ResolvedPeerDTO obj)
    {
        var resolved = _factory.Resolve<ResolvedPeerImpl>();
        resolved.Peer = _mapper.MapToTLObject<Peer, PeerDTO>(obj.Peer);
        resolved.Chats = _factory.Resolve<Vector<Chat>>();
        foreach (var c in obj.Chats)
        {
            resolved.Chats.Add(_mapper.MapToTLObject<Chat, ChatDTO>(c));
        }
        resolved.Users = _factory.Resolve<Vector<User>>();
        foreach (var u in obj.Users)
        {
            resolved.Users.Add(_mapper.MapToTLObject<User, UserDTO>(u));
        }

        return resolved;
    }
}