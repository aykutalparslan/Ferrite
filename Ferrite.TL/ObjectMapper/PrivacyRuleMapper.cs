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

public class PrivacyRuleMapper : ITLObjectMapper<PrivacyRule, PrivacyRuleDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public PrivacyRuleMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public PrivacyRuleDTO MapToDTO(PrivacyRule obj)
    {
        throw new NotImplementedException();
    }

    public PrivacyRule MapToTLObject(PrivacyRuleDTO obj)
    {
        if (obj.PrivacyRuleType == PrivacyRuleType.AllowAll)
        {
            return _factory.Resolve<PrivacyValueAllowAllImpl>();
        }
        else if (obj.PrivacyRuleType == PrivacyRuleType.AllowUsers)
        {
            var rule = _factory.Resolve<PrivacyValueAllowUsersImpl>();
            rule.Users = _factory.Resolve<VectorOfLong>();
            foreach (var p in obj.Peers)
            {
                rule.Users.Add(p);
            }

            return rule;
        }
        else if (obj.PrivacyRuleType == PrivacyRuleType.DisallowContacts)
        {
            return _factory.Resolve<PrivacyValueDisallowContactsImpl>();
        }
        else if (obj.PrivacyRuleType == PrivacyRuleType.DisallowAll)
        {
            return _factory.Resolve<PrivacyValueDisallowAllImpl>();
        }
        else if (obj.PrivacyRuleType == PrivacyRuleType.DisallowUsers)
        {
            var rule = _factory.Resolve<PrivacyValueDisallowUsersImpl>();
            rule.Users = _factory.Resolve<VectorOfLong>();
            foreach (var p in obj.Peers)
            {
                rule.Users.Add(p);
            }

            return rule;
        }
        else if (obj.PrivacyRuleType == PrivacyRuleType.AllowChatParticipants)
        {
            var rule = _factory.Resolve<PrivacyValueAllowChatParticipantsImpl>();
            rule.Chats = _factory.Resolve<VectorOfLong>();
            foreach (var p in obj.Peers)
            {
                rule.Chats.Add(p);
            }

            return rule;
        }
        else if (obj.PrivacyRuleType == PrivacyRuleType.DisallowChatParticipants)
        {
            var rule = _factory.Resolve<PrivacyValueDisallowChatParticipantsImpl>();
            rule.Chats = _factory.Resolve<VectorOfLong>();
            foreach (var p in obj.Peers)
            {
                rule.Chats.Add(p);
            }

            return rule;
        }
        return _factory.Resolve<PrivacyValueAllowContactsImpl>();
    }
}