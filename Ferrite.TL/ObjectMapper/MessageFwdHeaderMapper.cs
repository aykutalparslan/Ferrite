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

public class MessageFwdHeaderMapper : ITLObjectMapper<MessageFwdHeader, MessageFwdHeaderDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public MessageFwdHeaderMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public MessageFwdHeaderDTO MapToDTO(MessageFwdHeader obj)
    {
        throw new NotSupportedException();
    }

    public MessageFwdHeader MapToTLObject(MessageFwdHeaderDTO obj)
    {
        var fwdHeader = _factory.Resolve<MessageFwdHeaderImpl>();
        fwdHeader.Imported = obj.Imported;
        fwdHeader.Date = obj.Date;
        if(obj.FromId != null)
        {
            fwdHeader.FromId = _mapper.MapToTLObject<Peer, PeerDTO>(obj.FromId);
        }
        if(obj.FromName != null)
        {
            fwdHeader.FromName = obj.FromName;
        }
        if(obj.ChannelPost != null)
        {
            fwdHeader.ChannelPost = (int)obj.ChannelPost;
        }
        if(obj.PostAuthor != null)
        {
            fwdHeader.PostAuthor = obj.PostAuthor;
        }
        if(obj.SavedFromPeer != null)
        {
            fwdHeader.SavedFromPeer = _mapper.MapToTLObject<Peer, PeerDTO>(obj.SavedFromPeer);
        }
        if(obj.SavedFromMsgId != null)
        {
            fwdHeader.SavedFromMsgId = (int)obj.SavedFromMsgId;
        }
        if(obj.PsaType != null)
        {
            fwdHeader.PsaType = obj.PsaType;
        }
        return fwdHeader;
    }
}