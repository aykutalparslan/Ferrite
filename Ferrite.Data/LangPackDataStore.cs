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

using Cassandra;

namespace Ferrite.Data;

public class LangPackDataStore : ILangPackDataStore
{
    private readonly Cluster cluster;
    private readonly ISession session;
    private readonly string keySpace;

    public LangPackDataStore(string keyspace, params string[] hosts)
    {
        cluster = Cluster.Builder()
            .AddContactPoints(hosts)
            .Build();

        keySpace = keyspace;
        session = cluster.Connect();
        CreateSchema();
    }

    private void CreateSchema()
    {
        Dictionary<string, string> replication = new Dictionary<string, string>();
        replication.Add("class", "SimpleStrategy");
        replication.Add("replication_factor", "1");
        session.CreateKeyspaceIfNotExists(keySpace, replication);
        var statement = new SimpleStatement(
            "CREATE TABLE IF NOT EXISTS ferrite.lang_pack_languages (" +
            "lang_pack text," +
            "lang_code text," +
            "official boolean," +
            "rtl boolean," +
            "beta boolean," +
            "lang_name text," +
            "lang_native_name text," +
            "base_lang_code text," +
            "plural_code text," +
            "strings_count int," +
            "translated_count int," +
            "translations_url text," +
            "PRIMARY KEY ((lang_pack, lang_code)));");
        session.Execute(statement.SetKeyspace(keySpace));
        statement = new SimpleStatement(
            "CREATE TABLE IF NOT EXISTS ferrite.lang_pack_strings (" +
            "lang_pack text," +
            "lang_code text," +
            "string_key text," +
            "string_value text," +
            "zero_value text," +
            "one_value text," +
            "few_value text," +
            "many_value text," +
            "other_value text," +
            "string_type int," +
            "deleted boolean," +
            "PRIMARY KEY ((lang_pack, lang_code), string_key));");
        session.Execute(statement.SetKeyspace(keySpace));
    }

    public async Task<bool> SaveLanguageAsync(LangPackLanguage language)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> SaveLangPackDifferenceAsync(LangPackDifference difference)
    {
        throw new NotImplementedException();
    }

    public async Task<ICollection<LangPackLanguage>> GetLanguagesAsync(string? langPack)
    {
        throw new NotImplementedException();
    }

    public async Task<LangPackLanguage?> GetLanguagesAsync(string langPack, string langCode)
    {
        throw new NotImplementedException();
    }

    public async Task<LangPackDifference?> GetLangPackAsync(string langPack, string langCode)
    {
        throw new NotImplementedException();
    }

    public async Task<LangPackDifference?> GetDifferenceAsync(string langPack, string langCode, int fromVersion)
    {
        throw new NotImplementedException();
    }

    public async Task<ICollection<LangPackString>> GetStringsAsync(string langPack, string langCode, ICollection<string> keys)
    {
        throw new NotImplementedException();
    }
}