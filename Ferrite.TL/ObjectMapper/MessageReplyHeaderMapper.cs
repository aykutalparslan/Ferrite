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

public class MessageReplyHeaderMapper : ITLObjectMapper<MessageReplyHeader, MessageReplyHeaderDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public MessageReplyHeaderMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }

    public MessageReplyHeaderDTO MapToDTO(MessageReplyHeader obj)
    {
        throw new NotSupportedException();
    }

    public MessageReplyHeader MapToTLObject(MessageReplyHeaderDTO obj)
    {
        var replyHeader = _factory.Resolve<MessageReplyHeaderImpl>();
        replyHeader.ReplyToMsgId = obj.ReplyToMsgId;
        if(obj.ReplyToPeerId != null)
        {
            replyHeader.ReplyToPeerId = _mapper.MapToTLObject<Peer, PeerDTO>(obj.ReplyToPeerId);
        }
        if(obj.ReplyToTopId != null)
        {
            replyHeader.ReplyToTopId = (int)obj.ReplyToTopId;
        }
        return replyHeader;
    }
}