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

using Ferrite.Data.Updates;
using Ferrite.TL.currentLayer.updates;

namespace Ferrite.TL.ObjectMapper;

public class StateMapper : ITLObjectMapper<State, StateDTO>
{
    private readonly ITLObjectFactory _factory;
    public StateMapper(ITLObjectFactory factory)
    {
        _factory = factory;
    }
    public StateDTO MapToDTO(State obj)
    {
        throw new NotImplementedException();
    }

    public State MapToTLObject(StateDTO obj)
    {
        var state = _factory.Resolve<StateImpl>();
        state.Date = obj.Date;
        state.Pts = obj.Pts;
        state.Qts = obj.Qts;
        state.Seq = obj.Seq;
        state.UnreadCount = obj.UnreadCount;
        return state;
    }
}