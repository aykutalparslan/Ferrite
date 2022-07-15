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
using Ferrite.TL.currentLayer;

namespace Ferrite.TL.ObjectMapper;

public class PeerMapper : ITLObjectMapper<Peer, PeerDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public PeerMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public PeerDTO MapToDTO(Peer obj)
    {
        throw new NotImplementedException();
    }

    public Peer MapToTLObject(PeerDTO obj)
    {
        if (obj.PeerType == PeerType.User)
        {
            var peer = _factory.Resolve<PeerUserImpl>();
            peer.UserId = obj.PeerId;
            return peer;
        }
        else if (obj.PeerType == PeerType.Chat)
        {
            var peer = _factory.Resolve<PeerChatImpl>();
            peer.ChatId = obj.PeerId;
            return peer;
        }
        else if (obj.PeerType == PeerType.Channel)
        {
            var peer = _factory.Resolve<PeerChannelImpl>();
            peer.ChannelId = obj.PeerId;
            return peer;
        }
        else
        {
            throw new NotSupportedException();
        }
    }
}