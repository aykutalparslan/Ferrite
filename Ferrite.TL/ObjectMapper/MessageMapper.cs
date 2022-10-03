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

public class MessageMapper : ITLObjectMapper<Message, MessageDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public MessageMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public MessageDTO MapToDTO(Message obj)
    {
        throw new NotSupportedException();
    }

    public Message MapToTLObject(MessageDTO obj)
    {
        if (obj.MessageType == MessageType.Message)
        {
            var message = _factory.Resolve<MessageImpl>();
            message.Out = obj.Out;
            message.Silent = obj.Silent;
            message.Id = obj.Id;
            if(obj.FromId != null)
            {
                message.FromId = _mapper.MapToTLObject<Peer, PeerDTO>(obj.FromId);
            }
            message.PeerId = _mapper.MapToTLObject<Peer, PeerDTO>(obj.PeerId);
            if(obj.FwdFrom != null)
            {
                message.FwdFrom = _mapper.MapToTLObject<MessageFwdHeader, MessageFwdHeaderDTO>(obj.FwdFrom);
            }
            if(obj.ViaBotId != null)
            {
                message.ViaBotId = (long)obj.ViaBotId;
            }
            if(obj.ReplyTo != null)
            {
                message.ReplyTo = _mapper.MapToTLObject<MessageReplyHeader, MessageReplyHeaderDTO>(obj.ReplyTo);
            }
            message.Date = obj.Date;
            message.Message = obj.MessageText;
            if (obj.Media != null)
            {
                message.Media = _mapper.MapToTLObject<MessageMedia, MessageMediaDTO>(obj.Media);
            }
            if(obj.ReplyMarkup != null)
            {
                message.ReplyMarkup = _mapper.MapToTLObject<ReplyMarkup, ReplyMarkupDTO>(obj.ReplyMarkup);
            }
            if(obj.Entities != null)
            {
                message.Entities = _factory.Resolve<Vector<MessageEntity>>();
                foreach (var e in obj.Entities)
                {
                    var entity = _mapper.MapToTLObject<MessageEntity, MessageEntityDTO>(e);
                    message.Entities.Add(entity);
                }
            }
            if(obj.Views != null)
            {
                message.Views = (int)obj.Views;
            }
            if(obj.Forwards != null)
            {
                message.Forwards = (int)obj.Forwards;
            }
            //TODO: Implement MessageReplies mapping
            //if(obj.Replies != null)
            //{
            //    message.Replies = _mapper.MapToTLObject<MessageReplies, MessageRepliesDTO>(obj.Replies);
            //}
            if(obj.EditDate != null)
            {
                message.EditDate = (int)obj.EditDate;
            }
            if(obj.PostAuthor != null)
            {
                message.PostAuthor = obj.PostAuthor;
            }
            if(obj.GroupedId != null)
            {
                message.GroupedId = (long)obj.GroupedId;
            }
            //TODO: Implement MessageReactions mapping
            //if(obj.Reactions != null)
            //{
            //    message.Reactions = _mapper.MapToTLObject<MessageReactions, MessageReactionsDTO>(obj.Reactions);
            //}
            //TODO: Implement MessageReactions mapping
            // if(obj.RestrictionReason != null)
            // {
            //     message.RestrictionReason = _factory.Resolve<Vector<RestrictionReason>>();
            //     foreach (var r in obj.RestrictionReason)
            //     {
            //         var entity = _mapper.MapToTLObject<RestrictionReason, RestrictionReasonDTO>(r);
            //         message.RestrictionReason.Add(r);
            //     }
            // }
            if(obj.TtlPeriod != null)
            {
                message.TtlPeriod = (int)obj.TtlPeriod;
            }

            return message;
        }
        throw new NotSupportedException();
    }
}