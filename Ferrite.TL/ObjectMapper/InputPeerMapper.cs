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

public class InputPeerMapper : ITLObjectMapper<InputPeer, InputPeerDTO>
{
    public InputPeerDTO MapToDTO(InputPeer obj)
    {
        return new InputPeerDTO()
        {
            InputPeerType = obj.Constructor switch
            {
                currentLayer.TLConstructor.InputPeerChat => InputPeerType.Chat,
                currentLayer.TLConstructor.InputPeerChannel => InputPeerType.Channel,
                currentLayer.TLConstructor.InputPeerUserFromMessage => InputPeerType.UserFromMessage,
                currentLayer.TLConstructor.InputPeerChannelFromMessage => InputPeerType.ChannelFromMessage,
                _ => InputPeerType.User
            },
            UserId = obj.Constructor switch
            {
                currentLayer.TLConstructor.InputPeerUser => ((InputPeerUserImpl)obj).UserId,
                currentLayer.TLConstructor.InputPeerUserFromMessage => ((InputPeerUserFromMessageImpl)obj).UserId,
                _ => 0
            },
            AccessHash = obj.Constructor switch
            {
                currentLayer.TLConstructor.InputPeerUser => ((InputPeerUserImpl)obj).UserId,
                currentLayer.TLConstructor.InputPeerUserFromMessage =>
                    ((InputPeerUserImpl)((InputPeerUserFromMessageImpl)obj).Peer).AccessHash,
                _ => 0
            },
            ChatId = obj.Constructor == currentLayer.TLConstructor.InputPeerChat
                ? ((InputPeerChatImpl)obj).ChatId
                : 0,
            ChannelId = obj.Constructor switch
            {
                currentLayer.TLConstructor.InputPeerChannel => ((InputPeerChannelImpl)obj).ChannelId,
                currentLayer.TLConstructor.InputPeerChannelFromMessage => ((InputPeerChannelFromMessageImpl)obj).ChannelId,
                _ => 0
            },
        };
    }

    public InputPeer MapToTLObject(InputPeerDTO obj)
    {
        throw new NotImplementedException();
    }
}