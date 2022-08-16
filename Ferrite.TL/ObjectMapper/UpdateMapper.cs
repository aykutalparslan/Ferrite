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

public class UpdateMapper : ITLObjectMapper<Update, UpdateBase>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public UpdateMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public UpdateBase MapToDTO(Update obj)
    {
        throw new NotImplementedException();
    }

    public Update MapToTLObject(UpdateBase obj)
    {
        if (obj is UpdateMessageIdDTO updateMessageId)
        {
            var update = _factory.Resolve<UpdateMessageIDImpl>();
            update.Id = updateMessageId.Id;
            update.RandomId = updateMessageId.RandomId;
            return update;
        }
        else if (obj is UpdateReadHistoryInboxDTO readInbox)
        {
            var update = _factory.Resolve<UpdateReadHistoryInboxImpl>();
            update.Peer = _mapper.MapToTLObject<Peer, PeerDTO>(readInbox.Peer);
            update.Pts = readInbox.Pts;
            update.PtsCount = readInbox.PtsCount;
            update.MaxId = readInbox.MaxId;
            update.StillUnreadCount = readInbox.StillUnreadCount;
            if (readInbox.FolderId != null)
            {
                update.FolderId = (int)readInbox.FolderId;
            }
            return update;
        }
        else if (obj is UpdateReadHistoryOutboxDTO readOutbox)
        {
            var update = _factory.Resolve<UpdateReadHistoryOutboxImpl>();
            update.Peer = _mapper.MapToTLObject<Peer, PeerDTO>(readOutbox.Peer);
            update.Pts = readOutbox.Pts;
            update.PtsCount = readOutbox.PtsCount;
            update.MaxId = readOutbox.MaxId;
            return update;
        }
        else if (obj is UpdateNewMessageDTO newMessage)
        {
            var update = _factory.Resolve<UpdateNewMessageImpl>();
            update.Message = _mapper.MapToTLObject<Message, MessageDTO>(newMessage.Message);
            update.Pts = newMessage.Pts;
            update.PtsCount = newMessage.PtsCount;
            return update;
        }
        else if (obj is UpdateDeleteMessagesDTO deleteMessages)
        {
            var update = _factory.Resolve<UpdateDeleteMessagesImpl>();
            update.Messages = _factory.Resolve<VectorOfInt>();
            foreach (var m in deleteMessages.Messages)
            {
                update.Messages.Add(m);
            }
            update.Pts = deleteMessages.Pts;
            update.PtsCount = deleteMessages.PtsCount;
            return update;
        }
        throw new NotSupportedException("Update type is not supported.");
    }
}