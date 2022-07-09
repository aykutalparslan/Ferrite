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

public class InputUserMapper : ITLObjectMapper<InputUser, InputUserDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public InputUserMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public InputUserDTO MapToDTO(InputUser obj)
    {
        if (obj is InputUserImpl user)
        {
            return new Data.InputUserDTO()
            {
                InputUserType = InputUserType.User,
                UserId = user.UserId,
                AccessHash = user.AccessHash
            };
        }
        else if (obj is InputUserFromMessageImpl userFromMessage)
        {
            return new Data.InputUserDTO()
            {
                InputUserType = InputUserType.UserFromMessage,
                UserId = userFromMessage.UserId,
                MsgId = userFromMessage.MsgId,
                Peer = _mapper.MapToDTO<InputPeer, InputPeerDTO>(userFromMessage.Peer)
            };
        }
        else if (obj is InputUserSelfImpl userSelf)
        {
            return new Data.InputUserDTO()
            {
                InputUserType = InputUserType.Self
            };
        }
        return new Data.InputUserDTO()
        {
            InputUserType = InputUserType.Empty
        }; 
    }

    public InputUser MapToTLObject(InputUserDTO obj)
    {
        throw new NotImplementedException();
    }
}