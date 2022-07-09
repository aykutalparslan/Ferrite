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

public class ChatMapper : ITLObjectMapper<Chat, ChatDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public ChatMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public ChatDTO MapToDTO(Chat obj)
    {
        throw new NotImplementedException();
    }

    public Chat MapToTLObject(ChatDTO obj)
    {
        if (obj.ChatType == ChatType.Chat)
        {
            var chat = _factory.Resolve<ChatImpl>();
            chat.Id = obj.Id;
            chat.Title = obj.Title;
            chat.Photo = _factory.Resolve<ChatPhotoEmptyImpl>();
            chat.ParticipantsCount = obj.ParticipantsCount;
            chat.Date = obj.Date;
            chat.Version = obj.Version;
            return chat;
        }
        else if (obj.ChatType == ChatType.Channel)
        {
            var chat = _factory.Resolve<ChannelImpl>();
            chat.Id = obj.Id;
            chat.Title = obj.Title;
            chat.Photo = _factory.Resolve<ChatPhotoEmptyImpl>();
            chat.ParticipantsCount = obj.ParticipantsCount;
            chat.Date = obj.Date;
            return chat;
        }
        else if (obj.ChatType == ChatType.ChatForbidden)
        {
            var chat = _factory.Resolve<ChatForbiddenImpl>();
            chat.Id = obj.Id;
            chat.Title = obj.Title;
            return chat;
        }
        else if (obj.ChatType == ChatType.ChannelForbidden)
        {
            var chat = _factory.Resolve<ChannelForbiddenImpl>();
            chat.Id = obj.Id;
            chat.Title = obj.Title;
            chat.AccessHash = obj.AccessHash;
            return chat;
        }
        return _factory.Resolve<ChatEmptyImpl>();
    }
}