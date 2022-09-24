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
using MessagePack;

namespace Ferrite.Data.Repositories;

public class LangPackRepository : ILangPackRepository
{
    private readonly IKVStore _store;
    private readonly IKVStore _storeStrings;
    private bool loadingFromDisk = false;
    private Task? loadFromDisk;

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
    private async Task LoadFromDisk()
    {
        if (loadingFromDisk)
        {
            return;
        }

        loadingFromDisk = true;
        var iter = _store.Iterate("android");
        if (iter.FirstOrDefault() != null)
        {
            loadingFromDisk = false;
            return;
        }
        string[] langPacks = {"android", "ios", "tdesktop", "macos", "android_x" };
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
                SaveLanguage(langPack, language);
                using StreamReader rd2 = new StreamReader($"LangData/{langPack}-{language.LangCode}.json");
                LangPackDifferenceDTO difference = JsonSerializer.Deserialize<LangPackDifferenceDTO>(rd2.ReadToEnd());
                SaveLangPackDifference(langPack, difference);
            }
        }

        loadingFromDisk = false;
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

    public async ValueTask<List<LangPackLanguageDTO>> GetLanguagesAsync(string? langPack)
    {
        if (loadingFromDisk != null)
        {
            await loadFromDisk;
        }
        List<LangPackLanguageDTO> result = new List<LangPackLanguageDTO>();
        var iter = _store.Iterate(langPack);
        foreach (var langPackBytes in iter)
        {
            var language = MessagePackSerializer.Deserialize<LangPackLanguageDTO>(langPackBytes);
        }

        return result;
    }

    public async ValueTask<LangPackLanguageDTO?> GetLanguageAsync(string langPack, string langCode)
    {
        if (loadingFromDisk != null)
        {
            await loadFromDisk;
        }
        var langPackBytes = _store.Get(langPack, langCode);
        if (langPackBytes != null)
        {
            return MessagePackSerializer.Deserialize<LangPackLanguageDTO>(langPackBytes);
        }

        return default(LangPackLanguageDTO);
    }

    public async ValueTask<LangPackDifferenceDTO?> GetLangPackAsync(string langPack, string langCode)
    {
        if (loadingFromDisk != null)
        {
            await loadFromDisk;
        }
        int version = 0;
        return GetDifferenceInternal(langPack, langCode, version);
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

    public async ValueTask<LangPackDifferenceDTO?> GetDifferenceAsync(string langPack, string langCode, int fromVersion)
    {
        if (loadingFromDisk != null)
        {
            await loadFromDisk;
        }
        return GetDifferenceInternal(langPack, langCode, fromVersion);
    }

    public async ValueTask<List<LangPackStringDTO>> GetStringsAsync(string langPack, string langCode,
        ICollection<string> keys)
    {
        if (loadingFromDisk != null)
        {
            await loadFromDisk;
        }
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

        return result;
    }
}