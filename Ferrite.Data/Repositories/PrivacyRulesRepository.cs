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

using MessagePack;

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
    public bool PutPrivacyRules(long userId, InputPrivacyKey key, ICollection<PrivacyRuleDTO> rules)
    {
        foreach (var rule in rules)
        {
            var ruleBytes = MessagePackSerializer.Serialize(rule);
            _store.Put(ruleBytes, userId, (int)key, (int)rule.PrivacyRuleType);
        }

        return true;
    }

    public ICollection<PrivacyRuleDTO> GetPrivacyRules(long userId, InputPrivacyKey key)
    {
        List<PrivacyRuleDTO> rules = new();
        var iter = _store.Iterate(userId, (int)key);
        foreach (var ruleBytes in iter)
        {
            var rule = MessagePackSerializer.Deserialize<PrivacyRuleDTO>(ruleBytes);
            rules.Add(rule);
        }

        return rules;
    }

    public bool DeletePrivacyRules(long userId)
    {
        return _store.Delete(userId);
    }
}