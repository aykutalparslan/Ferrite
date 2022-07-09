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

public class InputNotifyPeerMapper : ITLObjectMapper<InputNotifyPeer, InputNotifyPeerDTO>
{
    private readonly IMapperContext _mapper;

    public InputNotifyPeerMapper(IMapperContext mapper)
    {
        _mapper = mapper;
    }
    public InputNotifyPeerDTO MapToDTO(InputNotifyPeer obj)
    {
        if (obj.Constructor == currentLayer.TLConstructor.InputNotifyPeer)
        {
            var peer = (InputNotifyPeerImpl)obj;
            return new Data.InputNotifyPeerDTO()
            {
                NotifyPeerType = InputNotifyPeerType.Peer,
                Peer = _mapper.MapToDTO<InputPeer, InputPeerDTO>(peer.Peer)
            };
        }
        if (obj.Constructor == currentLayer.TLConstructor.InputNotifyChats)
        {
            return new Data.InputNotifyPeerDTO()
            {
                NotifyPeerType = InputNotifyPeerType.Chats
            };
        }
        if (obj.Constructor == currentLayer.TLConstructor.InputNotifyUsers)
        {
            return new Data.InputNotifyPeerDTO()
            {
                NotifyPeerType = InputNotifyPeerType.Users
            };
        }
        if (obj.Constructor == currentLayer.TLConstructor.InputNotifyBroadcasts)
        {
            return new Data.InputNotifyPeerDTO()
            {
                NotifyPeerType = InputNotifyPeerType.Broadcasts
            };
        }
        throw new NotSupportedException($"{obj.Constructor} is not supported as InputNotifyPeer");
    }

    public InputNotifyPeer MapToTLObject(InputNotifyPeerDTO obj)
    {
        throw new NotImplementedException();
    }
}