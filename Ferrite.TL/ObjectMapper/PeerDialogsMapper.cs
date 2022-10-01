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
using Ferrite.Data.Messages;
using Ferrite.Data.Updates;
using Ferrite.TL.currentLayer;
using Ferrite.TL.currentLayer.messages;
using Ferrite.TL.currentLayer.updates;

namespace Ferrite.TL.ObjectMapper;

public class PeerDialogsMapper : ITLObjectMapper<PeerDialogs, PeerDialogsDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public PeerDialogsMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public PeerDialogsDTO MapToDTO(PeerDialogs obj)
    {
        throw new NotImplementedException();
    }

    public PeerDialogs MapToTLObject(PeerDialogsDTO obj)
    {
        var dialogs = _factory.Resolve<PeerDialogsImpl>();
        dialogs.Chats = _factory.Resolve<Vector<Chat>>();
        foreach (var c in obj.Chats)
        {
            dialogs.Chats.Add(_mapper.MapToTLObject<Chat, ChatDTO>(c));
        }
        dialogs.Dialogs = _factory.Resolve<Vector<Dialog>>();
        foreach (var d in obj.Dialogs)
        {
            dialogs.Dialogs.Add(_mapper.MapToTLObject<Dialog, DialogDTO>(d));
        }
        dialogs.Messages = _factory.Resolve<Vector<Message>>();
        foreach (var m in obj.Messages)
        {
            dialogs.Messages.Add(_mapper.MapToTLObject<Message, MessageDTO>(m));
        }
        dialogs.Users = _factory.Resolve<Vector<User>>();
        foreach (var u in obj.Users)
        {
            dialogs.Users.Add(_mapper.MapToTLObject<User, UserDTO>(u));
        }
        dialogs.State = _mapper.MapToTLObject<State, StateDTO>(obj.State);
        return dialogs;
    }
}