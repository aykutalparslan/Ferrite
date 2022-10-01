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
using Ferrite.Data.Updates;
using Ferrite.TL.currentLayer;
using Ferrite.TL.currentLayer.updates;

namespace Ferrite.TL.ObjectMapper;

public class DifferenceMapper  : ITLObjectMapper<Difference, DifferenceDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public DifferenceMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public DifferenceDTO MapToDTO(Difference obj)
    {
        throw new NotImplementedException();
    }

    public Difference MapToTLObject(DifferenceDTO obj)
    {
        var diff = _factory.Resolve<DifferenceImpl>();
        diff.NewMessages = _factory.Resolve<Vector<Message>>();
        diff.NewEncryptedMessages = _factory.Resolve<Vector<EncryptedMessage>>();
        //TODO: Map Encrypted Messages
        diff.OtherUpdates = _factory.Resolve<Vector<Update>>();
        diff.Chats = _factory.Resolve<Vector<Chat>>();
        diff.Users = _factory.Resolve<Vector<User>>();
        foreach (var c in obj.Chats)
        {
            diff.Chats.Add(_mapper.MapToTLObject<Chat, ChatDTO>(c));
        }
        foreach (var m in obj.NewMessages)
        {
            diff.NewMessages.Add(_mapper.MapToTLObject<Message, MessageDTO>(m));
        }
        foreach (var u in obj.Users)
        {
            diff.Users.Add(_mapper.MapToTLObject<User, UserDTO>(u));
        }
        diff.State = _mapper.MapToTLObject<State, StateDTO>(obj.State);
        return diff;
    }
}