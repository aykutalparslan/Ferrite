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

public class InputMessageMapper : ITLObjectMapper<InputMessage, InputMessageDTO>
{
    public InputMessageDTO MapToDTO(InputMessage obj)
    {
        if (obj is InputMessageIDImpl id)
        {
            return new InputMessageDTO(InputMessageType.Id, id.Id);
        }
        else if(obj is InputMessageReplyToImpl replyTo)
        {
            return new InputMessageDTO(InputMessageType.ReplyTo, replyTo.Id);
        }
        else if (obj is InputMessagePinnedImpl)
        {
            return new InputMessageDTO(InputMessageType.ReplyTo);
        }
        else if (obj is InputMessageCallbackQueryImpl callback)
        {
            return new InputMessageDTO(InputMessageType.ReplyTo, callback.Id, callback.QueryId);
        }
        throw new NotSupportedException();
    }

    public InputMessage MapToTLObject(InputMessageDTO obj)
    {
        throw new NotSupportedException();
    }
}