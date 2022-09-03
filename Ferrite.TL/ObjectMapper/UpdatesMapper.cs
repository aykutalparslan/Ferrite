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

public class UpdatesMapper : ITLObjectMapper<Updates, UpdatesBase>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public UpdatesMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public UpdatesBase MapToDTO(Updates obj)
    {
        throw new NotImplementedException();
    }

    public Updates MapToTLObject(UpdatesBase obj)
    {
        if (obj is UpdateShortSentMessageDTO sentMessage)
        {
            var update = _factory.Resolve<UpdateShortSentMessageImpl>();
            update.Out = sentMessage.Out;
            update.Id = sentMessage.Id;
            update.Pts = sentMessage.Pts;
            update.PtsCount = sentMessage.PtsCount;
            update.Date = sentMessage.Date;
            if (sentMessage.Media != null)
            {
                update.Media = _mapper.MapToTLObject<MessageMedia, MessageMediaDTO>(sentMessage.Media);
            }
            if (sentMessage.Entities != null)
            {
                update.Entities = _factory.Resolve<Vector<MessageEntity>>();
                foreach (var e in sentMessage.Entities)
                {
                    update.Entities.Add(_mapper.MapToTLObject<MessageEntity, MessageEntityDTO>(e));
                }
            }

            if (sentMessage.TtlPeriod != null)
            {
                update.TtlPeriod = (int)sentMessage.TtlPeriod;
            }
            return update;
        }
        else if (obj is UpdatesDTO updates)
        {
            var update = _factory.Resolve<UpdatesImpl>();
            update.Seq = updates.Seq;
            update.Date = updates.Date;
            update.Updates = _factory.Resolve<Vector<Update>>();
            foreach (var u in updates.Updates)
            {
                update.Updates.Add(_mapper.MapToTLObject<Update, UpdateBase>(u));
            }
            update.Users = _factory.Resolve<Vector<User>>();
            foreach (var u in updates.Users)
            {
                update.Users.Add(_mapper.MapToTLObject<User, UserDTO>(u));
            }
            update.Chats = _factory.Resolve<Vector<Chat>>();
            foreach (var c in updates.Chats)
            {
                update.Chats.Add(_mapper.MapToTLObject<Chat, ChatDTO>(c));
            }
            return update;
        }
        else if (obj is UpdateShortDTO updateShort)
        {
            var update = _factory.Resolve<UpdateShortImpl>();
            update.Date = updateShort.Date;
            update.Update = _mapper.MapToTLObject<Update, UpdateBase>(updateShort.Update);
            return update;
        }
        throw new NotSupportedException("Updates type is not supported.");
    }
}