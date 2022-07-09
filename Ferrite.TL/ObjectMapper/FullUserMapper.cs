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

public class FullUserMapper : ITLObjectMapper<UserFull, UserFullDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public FullUserMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public UserFullDTO MapToDTO(UserFull obj)
    {
        throw new NotImplementedException();
    }

    public UserFull MapToTLObject(UserFullDTO obj)
    {
        var fullUser = _factory.Resolve<UserFullImpl>();
        if (obj.About is { Length: > 0 })
        {
            fullUser.About = obj.About;
        }
        fullUser.Blocked = obj.Blocked;
        fullUser.Id = obj.Id;
        fullUser.Settings = _mapper.MapToTLObject<PeerSettings, PeerSettingsDTO>(obj.Settings);
        fullUser.NotifySettings = _mapper.MapToTLObject<PeerNotifySettings, PeerNotifySettingsDTO>(obj.NotifySettings);
        fullUser.Blocked = obj.Blocked;
        fullUser.PhoneCallsAvailable = obj.PhoneCallsAvailable;
        fullUser.PhoneCallsPrivate = obj.PhoneCallsPrivate;
        fullUser.CommonChatsCount = obj.CommonChatsCount;
        if (obj.ProfilePhoto != null)
        {
            fullUser.ProfilePhoto = _mapper.MapToTLObject<Photo, PhotoDTO>(obj.ProfilePhoto);
        }

        return fullUser;
    }
}