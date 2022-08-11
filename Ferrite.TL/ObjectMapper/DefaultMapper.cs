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

using System.Collections.Concurrent;
using Autofac;
using Ferrite.Data;
using Ferrite.TL.currentLayer;

namespace Ferrite.TL.ObjectMapper;

public class DefaultMapper : IMapperContext
{
    private readonly ConcurrentDictionary<Type, object> _mappers = new();

    public DefaultMapper(ITLObjectFactory factory)
    {
        _mappers.TryAdd(typeof(InputPeer), new InputPeerMapper(factory, this));
        _mappers.TryAdd(typeof(UserFull), new FullUserMapper(factory, this));
        _mappers.TryAdd(typeof(InputNotifyPeer), new InputNotifyPeerMapper(this));
        _mappers.TryAdd(typeof(InputUser), new InputUserMapper(factory, this));
        _mappers.TryAdd(typeof(PeerNotifySettings), new PeerNotifySettingsMapper(factory));
        _mappers.TryAdd(typeof(PeerSettings), new PeerSettingsMapper(factory));
        _mappers.TryAdd(typeof(Photo), new PhotoMapper(factory));
        _mappers.TryAdd(typeof(User), new UserMapper(factory));
        _mappers.TryAdd(typeof(PrivacyRule), new PrivacyRuleMapper(factory, this));
        _mappers.TryAdd(typeof(Chat), new ChatMapper(factory,this));
        _mappers.TryAdd(typeof(InputPrivacyRule), new PrivacyRuleMapper(factory, this));
        _mappers.TryAdd(typeof(ReplyMarkup), new ReplyMarkupMapper(factory, this));
        _mappers.TryAdd(typeof(MessageEntity), new MessageEntityMapper(factory, this));
        _mappers.TryAdd(typeof(Update), new UpdateMapper(factory, this));
        _mappers.TryAdd(typeof(Updates), new UpdatesMapper(factory, this));
        _mappers.TryAdd(typeof(Message), new MessageMapper(factory, this));
        _mappers.TryAdd(typeof(Peer), new PeerMapper(factory, this));
        _mappers.TryAdd(typeof(MessageFwdHeader), new MessageFwdHeaderMapper(factory, this));
        _mappers.TryAdd(typeof(MessageReplyHeader), new MessageReplyHeaderMapper(factory, this));
        _mappers.TryAdd(typeof(MessageMedia), new MessageMediaMapper(factory, this));
        _mappers.TryAdd(typeof(InputMessage), new InputMessageMapper());
        _mappers.TryAdd(typeof(Dialog), new DialogMapper(factory, this));
    }
    
    public DTOType MapToDTO<TLType, DTOType>(TLType obj) where TLType : ITLObject
    {
        var t = typeof(TLType);
        if (!_mappers.ContainsKey(t)) throw new NotSupportedException();
        var mapper = (ITLObjectMapper<TLType, DTOType>)_mappers[typeof(TLType)];
        return mapper.MapToDTO(obj);
    }

    public TLType MapToTLObject<TLType, DTOType>(DTOType obj) where TLType : ITLObject
    {
        var t = typeof(TLType);
        if (!_mappers.ContainsKey(t)) throw new NotSupportedException();
        var mapper = (ITLObjectMapper<TLType, DTOType>)_mappers[typeof(TLType)];
        return mapper.MapToTLObject(obj);
    }
}