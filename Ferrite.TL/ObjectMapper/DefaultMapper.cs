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
using Ferrite.TL.currentLayer;

namespace Ferrite.TL.ObjectMapper;

public class DefaultMapper : IMapperContext
{
    private readonly ConcurrentDictionary<Type, object> _mappers = new();

    public DefaultMapper()
    {
        _mappers.TryAdd(typeof(InputPeer), new InputPeerMapper());
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