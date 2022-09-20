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

public class LangPackRepository : ILangPackRepository
{
    private readonly IKVStore _store;
    private readonly IKVStore _storeStrings;

    public LangPackRepository(IKVStore store, IKVStore storeStrings)
    {
        _store = store;
        _store.SetSchema(new TableDefinition("ferrite", "lang_packs",
            new KeyDefinition("pk",
                new DataColumn { Name = "lang_pack", Type = DataType.String },
                new DataColumn { Name = "lang_code", Type = DataType.String })));
        _storeStrings = storeStrings;
        _storeStrings.SetSchema(new TableDefinition("ferrite", "lang_pack_strings",
            new KeyDefinition("pk",
                new DataColumn { Name = "lang_pack", Type = DataType.String },
                new DataColumn { Name = "lang_code", Type = DataType.String })));
    }
    public bool SaveLanguage(string langPack, LangPackLanguageDTO languageDto)
    {
        var langPackBytes = MessagePackSerializer.Serialize(languageDto);
        return _store.Put(langPackBytes, langPack, languageDto.LangCode);
    }

    public bool SaveLangPackDifference(string langPack, LangPackDifferenceDTO difference)
    {
        var differenceBytes = MessagePackSerializer.Serialize(difference);
        return _storeStrings.Put(differenceBytes, langPack, difference.LangCode);
    }

    public ValueTask<List<LangPackLanguageDTO>> GetLanguagesAsync(string? langPack)
    {
        List<LangPackLanguageDTO> result = new List<LangPackLanguageDTO>();
        var iter = _store.Iterate(langPack);
        foreach (var langPackBytes in iter)
        {
            var language = MessagePackSerializer.Deserialize<LangPackLanguageDTO>(langPackBytes);
        }

        return ValueTask.FromResult(result);
    }

    public ValueTask<LangPackLanguageDTO?> GetLanguageAsync(string langPack, string langCode)
    {
        var langPackBytes = _store.Get(langPack, langCode);
        if (langPackBytes != null)
        {
            return ValueTask.FromResult(MessagePackSerializer.Deserialize<LangPackLanguageDTO>(langPackBytes));
        }

        return ValueTask.FromResult(default(LangPackLanguageDTO));
    }

    public ValueTask<LangPackDifferenceDTO?> GetLangPackAsync(string langPack, string langCode)
    {
        int version = 0;
        return ValueTask.FromResult(GetDifferenceInternal(langPack, langCode, version));
    }

    private LangPackDifferenceDTO? GetDifferenceInternal(string langPack, string langCode, int version)
    {
        int currentVersion = version;
        var iter = _storeStrings.Iterate(langPack, langCode);
        Dictionary<string, LangPackStringDTO> strings = new();
        foreach (var differenceBytes in iter)
        {
            var difference = MessagePackSerializer.Deserialize<LangPackDifferenceDTO>(differenceBytes);
            if (difference.Version > currentVersion)
            {
                currentVersion = difference.Version;
                foreach (var str in difference.Strings)
                {
                    strings.Add(str.Key, str);
                }
            }
        }

        if (version == 0 && strings.Count == 0) return null;

        var diff = new LangPackDifferenceDTO()
        {
            Version = version,
            FromVersion = version,
            LangCode = langCode,
            Strings = strings.Values,
        };
        return diff;
    }

    public ValueTask<LangPackDifferenceDTO?> GetDifferenceAsync(string langPack, string langCode, int fromVersion)
    {
        return ValueTask.FromResult(GetDifferenceInternal(langPack, langCode, fromVersion));
    }

    public ValueTask<List<LangPackStringDTO>> GetStringsAsync(string langPack, string langCode,
        ICollection<string> keys)
    {
        List<LangPackStringDTO> result = new List<LangPackStringDTO>();
        var diff = GetDifferenceInternal(langPack, langCode, 0);
        if (diff == null)
        {
            foreach (var str in diff.Strings)
            {
                if (keys.Contains(str.Key))
                {
                    result.Add(str);
                }
            }
        }

        return ValueTask.FromResult(result);
    }
}