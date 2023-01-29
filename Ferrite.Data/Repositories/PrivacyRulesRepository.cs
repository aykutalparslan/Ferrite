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

using System.Runtime.InteropServices;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150;

namespace Ferrite.Data.Repositories;

public class PrivacyRulesRepository : IPrivacyRulesRepository
{
    private readonly IKVStore _store;
    public PrivacyRulesRepository(IKVStore store)
    {
        _store = store;
        _store.SetSchema(new TableDefinition("ferrite", "privacy_rules",
            new KeyDefinition("pk",
                new DataColumn { Name = "user_id", Type = DataType.Long },
                new DataColumn { Name = "privacy_key", Type = DataType.Int },
                new DataColumn { Name = "privacy_rule_type", Type = DataType.Int })));
    }
    public bool PutPrivacyRules(long userId, InputPrivacyKey key, Vector rules)
    {
        int count = rules.Count;
        for(int i = 0 ; i < count; i++)
        {
            var rule = rules.ReadTLObject();
            int constructor = MemoryMarshal.Read<int>(rule);
            
            _store.Put(rule.ToArray(), userId, (int)key, (int)GetPrivacyValueType(constructor));
        }

        return true;
    }
    
    private PrivacyRuleType GetPrivacyValueType(int constructor) => constructor switch
    {
        Constructors.layer150_PrivacyValueAllowContacts => PrivacyRuleType.AllowContacts,
        Constructors.layer150_PrivacyValueAllowUsers => PrivacyRuleType.AllowUsers,
        Constructors.layer150_PrivacyValueDisallowContacts => PrivacyRuleType.DisallowContacts,
        Constructors.layer150_PrivacyValueDisallowAll => PrivacyRuleType.DisallowAll,
        Constructors.layer150_PrivacyValueDisallowUsers => PrivacyRuleType.DisallowUsers,
        Constructors.layer150_PrivacyValueAllowChatParticipants => PrivacyRuleType.AllowChatParticipants,
        Constructors.layer150_PrivacyValueDisallowChatParticipants => PrivacyRuleType.DisallowChatParticipants,
        _ => PrivacyRuleType.AllowAll
    };

    public ValueTask<ICollection<TLPrivacyRule>> GetPrivacyRulesAsync(long userId, InputPrivacyKey key)
    {
        List<TLPrivacyRule> rules = new();
        var iter = _store.Iterate(userId, (int)key);
        foreach (var ruleBytes in iter)
        {
            rules.Add(new TLPrivacyRule(ruleBytes, 0, ruleBytes.Length));
        }

        return new ValueTask<ICollection<TLPrivacyRule>>(rules);
    }

    public bool DeletePrivacyRules(long userId)
    {
        return _store.Delete(userId);
    }
}