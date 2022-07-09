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

using System.Text.Json;
using Cassandra;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Ferrite.Data;

public class LangPackDataStore : ILangPackDataStore
{
    private readonly Cluster cluster;
    private readonly ISession session;
    private readonly string keySpace;
    private bool loadingFromDisk = false;
    private Task? loadFromDisk;

    public LangPackDataStore(string keyspace, params string[] hosts)
    {
        cluster = Cluster.Builder()
            .AddContactPoints(hosts)
            .Build();

        keySpace = keyspace;
        session = cluster.Connect();
        CreateSchema();
        loadFromDisk = LoadFromDisk();
    }

    private async Task LoadFromDisk()
    {
        if (loadingFromDisk)
        {
            return;
        }

        loadingFromDisk = true;
        string[] langPacks = {"android", "ios", "tdesktop", "macos", "android_x" };
        var statement = new SimpleStatement(
            "SELECT * FROM ferrite.lang_pack_languages WHERE lang_pack = ?;",
            "android");
        statement = statement.SetKeyspace(keySpace);
        var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
        foreach (var row in results)
        {
            loadingFromDisk = false;
            return;
        }
        foreach (var langPack in langPacks)
        {
            using StreamReader rd = new StreamReader($"LangData/{langPack}-languages.json");
            var options = new JsonSerializerOptions
            {
                IncludeFields = true
            };
            LangPackLanguageDTO[] languages = JsonSerializer.Deserialize<LangPackLanguageDTO[]>(rd.ReadToEnd());
            foreach (LangPackLanguageDTO language in languages)
            {
                await SaveLanguageAsync(langPack, language);
                using StreamReader rd2 = new StreamReader($"LangData/{langPack}-{language.LangCode}.json");
                LangPackDifferenceDTO difference = JsonSerializer.Deserialize<LangPackDifferenceDTO>(rd2.ReadToEnd());
                await SaveLangPackDifferenceAsync(langPack, difference);
            }
        }

        loadingFromDisk = false;
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
            "translations_url text," +
            "PRIMARY KEY (lang_pack, lang_code));");
        session.Execute(statement.SetKeyspace(keySpace));
        statement = new SimpleStatement(
            "CREATE TABLE IF NOT EXISTS ferrite.lang_pack_strings (" +
            "lang_pack text," +
            "lang_code text," +
            "string_key text," +
            "version int," +
            "string_value text," +
            "zero_value text," +
            "one_value text," +
            "two_value text," +
            "few_value text," +
            "many_value text," +
            "other_value text," +
            "string_type int," +
            "PRIMARY KEY (lang_pack, lang_code, string_key, version));");
        session.Execute(statement.SetKeyspace(keySpace));
    }

    public async Task<bool> SaveLanguageAsync(string langPack, LangPackLanguageDTO languageDto)
    {
        var statement = new SimpleStatement(
            "UPDATE ferrite.lang_pack_languages SET official = ?, rtl = ?, " +
            "beta = ?, lang_name = ?, lang_native_name = ?, base_lang_code = ?, " +
            "plural_code = ?, translations_url = ? " +
            " WHERE lang_pack = ? AND lang_code = ?;",
            languageDto.Official, languageDto.Rtl, languageDto.Beta, languageDto.Name,
            languageDto.NativeName, languageDto.BaseLangCode, languageDto.PluralCode,
            languageDto.TranslationsUrl, langPack, languageDto.LangCode).SetKeyspace(keySpace);
        await session.ExecuteAsync(statement);
        return true;
    }

    public async Task<bool> SaveLangPackDifferenceAsync(string langPack, LangPackDifferenceDTO difference)
    {
        foreach (var str in difference.Strings)
        {
            var statement = new SimpleStatement(
                "UPDATE ferrite.lang_pack_strings SET string_value = ?, zero_value = ?, " +
                "one_value = ?, two_value = ?, few_value = ?, many_value = ?, other_value = ?, " +
                "string_type = ? " +
                " WHERE lang_pack = ? AND lang_code = ? AND string_key = ? AND version = ?;", str.Value, str.ZeroValue,
                str.OneValue, str.TwoValue, str.FewValue, str.ManyValue, str.OtherValue, (int)str.StringType,
                langPack, difference.LangCode, str.Key, difference.Version).SetKeyspace(keySpace);
            await session.ExecuteAsync(statement.SetKeyspace(keySpace));
        }
        
        return true;
    }

    public async Task<ICollection<LangPackLanguageDTO>> GetLanguagesAsync(string? langPack)
    {
        if (loadingFromDisk != null)
        {
            await loadFromDisk;
        }
        
        List<LangPackLanguageDTO> languages = new();
        
        var statement = new SimpleStatement(
            "SELECT * FROM ferrite.lang_pack_languages WHERE lang_pack = ?;",
            langPack);
        statement = statement.SetKeyspace(keySpace);

        var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
        foreach (var row in results)
        {
            var statement2 = new SimpleStatement(
                "SELECT COUNT(*) FROM ferrite.lang_pack_strings WHERE lang_pack = ? AND lang_code = ?;",
                langPack, row.GetValue<string>("lang_code"));
            var results2 = await session.ExecuteAsync(statement2.SetKeyspace(keySpace));
            int stringsCount = 0;
            foreach (var row2 in results2)
            {
                stringsCount = (int)row2.GetValue<long>(0);
            }
            languages.Add(new LangPackLanguageDTO()
            {
                LangCode = row.GetValue<string>("lang_code"),
                Official = row.GetValue<bool>("official"),
                Beta = row.GetValue<bool>("beta"),
                Rtl = row.GetValue<bool>("rtl"),
                Name = row.GetValue<string>("lang_name"),
                NativeName = row.GetValue<string>("lang_native_name"),
                BaseLangCode = row.GetValue<string>("base_lang_code"),
                PluralCode = row.GetValue<string>("plural_code"),
                TranslationsUrl = row.GetValue<string>("translations_url"),
                StringsCount = stringsCount,
            });
        }
        return languages;
    }

    public async Task<LangPackLanguageDTO?> GetLanguageAsync(string langPack, string langCode)
    {
        if (loadFromDisk != null)
        {
            await loadFromDisk;
        }
        LangPackLanguageDTO? lang = null;
        var statement = new SimpleStatement(
            "SELECT COUNT(*) FROM ferrite.lang_pack_strings WHERE lang_pack = ? AND lang_code;",
            langPack, langCode);
        var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
        int stringsCount = 0;
        foreach (var row in results)
        {
            stringsCount = row.GetValue<int>(0);
        }
        statement = new SimpleStatement(
            "SELECT * FROM ferrite.lang_pack_languages WHERE lang_pack = ? AND lang_code = ?;",
            langPack, langCode);
        statement = statement.SetKeyspace(keySpace);
        results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
        foreach (var row in results)
        {
            lang = new LangPackLanguageDTO()
            {
                LangCode = row.GetValue<string>("lang_code"),
                Official = row.GetValue<bool>("official"),
                Beta = row.GetValue<bool>("beta"),
                Rtl = row.GetValue<bool>("rtl"),
                Name = row.GetValue<string>("lang_name"),
                NativeName = row.GetValue<string>("lang_native_name"),
                BaseLangCode = row.GetValue<string>("base_lang_code"),
                PluralCode = row.GetValue<string>("plural_code"),
                TranslationsUrl = row.GetValue<string>("translations_url"),
                StringsCount = stringsCount,
            };
        }
        return lang;
    }

    public async Task<LangPackDifferenceDTO?> GetLangPackAsync(string langPack, string langCode)
    {
        if (loadFromDisk != null)
        {
            await loadFromDisk;
        }
        int version = 0;
        List<LangPackStringDTO> strings = new();
        var statement = new SimpleStatement(
            "SELECT * FROM ferrite.lang_pack_strings WHERE lang_pack = ? AND lang_code = ?;",
            langPack, langCode);
        var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
        foreach (var row in results)
        {
            LangPackStringType type = (LangPackStringType)row.GetValue<int>("string_type");
            int strVersion = row.GetValue<int>("version");
            if (strVersion > version)
            {
                version = strVersion;
            }
            if (type == LangPackStringType.Default)
            {
                strings.Add(new LangPackStringDTO()
                {
                    StringType = type,
                    Key = row.GetValue<string>("string_key"),
                    Value = row.GetValue<string>("string_value"),
                });
            }
            else if (type == LangPackStringType.Pluralized)
            {
                strings.Add(new LangPackStringDTO()
                {
                    StringType = type,
                    Key = row.GetValue<string>("string_key"),
                    ZeroValue = row.GetValue<string>("zero_value"),
                    OneValue = row.GetValue<string>("one_value"),
                    TwoValue = row.GetValue<string>("two_value"),
                    FewValue = row.GetValue<string>("few_value"),
                    ManyValue = row.GetValue<string>("many_value"),
                    OtherValue = row.GetValue<string>("other_value"),
                });
            }
            else if (type == LangPackStringType.Deleted)
            {
                strings.Add(new LangPackStringDTO()
                {
                    StringType = type,
                    Key = row.GetValue<string>("string_key"),
                });
            }
        }
        
        return new LangPackDifferenceDTO
        {
            LangCode = langCode,
            FromVersion = 0,
            Version = version,
            Strings = strings
        };
    }

    public async Task<LangPackDifferenceDTO?> GetDifferenceAsync(string langPack, string langCode, int fromVersion)
    {
        if (loadFromDisk != null)
        {
            await loadFromDisk;
        }
        int version = 0;
        List<LangPackStringDTO> strings = new();
        var statement = new SimpleStatement(
            "SELECT * FROM ferrite.lang_pack_strings WHERE lang_pack = ? AND lang_code = ? AND version > ?;",
            langPack, langCode, fromVersion);
        var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
        foreach (var row in results)
        {
            LangPackStringType type = (LangPackStringType)row.GetValue<int>("string_type");
            int strVersion = row.GetValue<int>("version");
            if (strVersion > version)
            {
                version = strVersion;
            }
            if (type == LangPackStringType.Default)
            {
                strings.Add(new LangPackStringDTO()
                {
                    StringType = type,
                    Key = row.GetValue<string>("string_key"),
                    Value = row.GetValue<string>("string_value"),
                });
            }
            else if (type == LangPackStringType.Pluralized)
            {
                strings.Add(new LangPackStringDTO()
                {
                    StringType = type,
                    Key = row.GetValue<string>("string_key"),
                    ZeroValue = row.GetValue<string>("zero_value"),
                    OneValue = row.GetValue<string>("one_value"),
                    TwoValue = row.GetValue<string>("two_value"),
                    FewValue = row.GetValue<string>("few_value"),
                    ManyValue = row.GetValue<string>("many_value"),
                    OtherValue = row.GetValue<string>("other_value"),
                });
            }
            else if (type == LangPackStringType.Deleted)
            {
                strings.Add(new LangPackStringDTO()
                {
                    StringType = type,
                    Key = row.GetValue<string>("string_key"),
                });
            }
        }
        
        return new LangPackDifferenceDTO
        {
            LangCode = langCode,
            FromVersion = fromVersion,
            Version = version > fromVersion ? version : fromVersion,
            Strings = strings
        };
    }

    public async Task<ICollection<LangPackStringDTO>> GetStringsAsync(string langPack, string langCode, ICollection<string> keys)
    {
        if (loadFromDisk != null)
        {
            await loadFromDisk;
        }
        List<LangPackStringDTO> strings = new();
        foreach (var key in keys)
        {
            var statement = new SimpleStatement(
                "SELECT * FROM ferrite.lang_pack_strings WHERE lang_pack = ? AND lang_code = ? AND string_key = ?;",
                langPack, langCode, key);
            var results = await session.ExecuteAsync(statement.SetKeyspace(keySpace));
            foreach (var row in results)
            {
                LangPackStringType type = (LangPackStringType)row.GetValue<int>("string_type");
                if (type == LangPackStringType.Default)
                {
                    strings.Add(new LangPackStringDTO()
                    {
                        StringType = type,
                        Key = row.GetValue<string>("string_key"),
                        Value = row.GetValue<string>("string_value"),
                    });
                }
                else if (type == LangPackStringType.Pluralized)
                {
                    strings.Add(new LangPackStringDTO()
                    {
                        StringType = type,
                        Key = row.GetValue<string>("string_key"),
                        ZeroValue = row.GetValue<string>("zero_value"),
                        OneValue = row.GetValue<string>("one_value"),
                        TwoValue = row.GetValue<string>("two_value"),
                        FewValue = row.GetValue<string>("few_value"),
                        ManyValue = row.GetValue<string>("many_value"),
                        OtherValue = row.GetValue<string>("other_value"),
                    });
                }
                else if (type == LangPackStringType.Deleted)
                {
                    strings.Add(new LangPackStringDTO()
                    {
                        StringType = type,
                        Key = row.GetValue<string>("string_key"),
                    });
                }
            }
        }
        return strings;
    }
}