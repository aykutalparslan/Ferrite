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

public class InputPrivacyRuleMapper : ITLObjectMapper<InputPrivacyRule, PrivacyRuleDTO>
{
    public PrivacyRuleDTO MapToDTO(InputPrivacyRule obj)
    {
        if (obj.Constructor == currentLayer.TLConstructor.InputPrivacyValueAllowContacts)
        {
            return new Data.PrivacyRuleDTO()
                { PrivacyRuleType = PrivacyRuleType.AllowContacts };
        }
        else if (obj.Constructor == currentLayer.TLConstructor.InputPrivacyValueAllowAll)
        {
            return new Data.PrivacyRuleDTO()
                { PrivacyRuleType = PrivacyRuleType.AllowAll };
        }
        else if (obj.Constructor == currentLayer.TLConstructor.InputPrivacyValueAllowUsers)
        {
            List<long> userIds = new();
            foreach (var user in ((InputPrivacyValueAllowUsersImpl)obj).Users)
            {
                userIds.Add(user.GetUserId());
            }

            return new Data.PrivacyRuleDTO()
            {
                PrivacyRuleType = PrivacyRuleType.AllowUsers,
                Peers = userIds
            };
        }
        else if (obj.Constructor == currentLayer.TLConstructor.InputPrivacyValueDisallowContacts)
        {
            return new Data.PrivacyRuleDTO()
                { PrivacyRuleType = PrivacyRuleType.DisallowContacts };
        }
        else if (obj.Constructor == currentLayer.TLConstructor.InputPrivacyValueDisallowAll)
        {
            return new Data.PrivacyRuleDTO()
                { PrivacyRuleType = PrivacyRuleType.DisallowAll };
        }
        else if (obj.Constructor == currentLayer.TLConstructor.InputPrivacyValueDisallowUsers)
        {
            List<long> userIds = new();
            foreach (var user in ((InputPrivacyValueAllowUsersImpl)obj).Users)
            {
                userIds.Add(user.GetUserId());
            }

            return new Data.PrivacyRuleDTO()
            {
                PrivacyRuleType = PrivacyRuleType.DisallowUsers,
                Peers = userIds
            };
        }
        else if (obj.Constructor == currentLayer.TLConstructor.InputPrivacyValueAllowChatParticipants)
        {
            return new Data.PrivacyRuleDTO()
            {
                PrivacyRuleType = PrivacyRuleType.AllowChatParticipants,
                Peers = ((InputPrivacyValueAllowChatParticipantsImpl)obj).Chats
            };
        }

        return new Data.PrivacyRuleDTO()
        {
            PrivacyRuleType = PrivacyRuleType.DisallowChatParticipants,
            Peers = ((InputPrivacyValueDisallowChatParticipantsImpl)obj).Chats
        };
    }

    public InputPrivacyRule MapToTLObject(PrivacyRuleDTO obj)
    {
        throw new NotImplementedException();
    }
}