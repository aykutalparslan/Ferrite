//
//  Project Ferrite is an Implementation Telegram Server API
//  Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using Ferrite.Data;

namespace Ferrite.Services;

//TODO: Implement a fully featured languge pack service
public class LangPackService : ILangPackService
{
    private readonly ConcurrentDictionary<string, string> _androidLangPack;
    public static string Android = "android";
    public static string English = "en";
    public static string Zero = "_zero";
    public static string One = "_one";
    public static string Two = "_two";
    public static string Few = "_few";
    public static string Many = "_many";
    public static string Other = "_other";
    public LangPackService()
    {
        StreamReader sr = new StreamReader("LanguagePacks/android_en_v20699265.json");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };
        var android = JsonSerializer.Deserialize<ConcurrentDictionary<string, string>>(sr.ReadToEnd(), options);
        if(android != null)
        {
            _androidLangPack = android;
        }
        else
        {
            _androidLangPack = new ConcurrentDictionary<string, string>();
        }
    }

    public Task<IDictionary<string, string>?> GetDifferenceAsync(string langPack, string langCode, int fromVersion)
    {
        throw new NotImplementedException();
    }

    public Task<IDictionary<string, string>?> GetLangPackAsync(string langPack, string langCode)
    {
        throw new NotImplementedException();
    }

    public Task<LangPackLanguage?> GetLanguageAsync(string langPack, string langCode)
    {
        throw new NotImplementedException();
    }

    public Task<List<LangPackLanguage>> GetLanguagesAsync(string langPack)
    {
        throw new NotImplementedException();
    }

    public async Task<IDictionary<string, string>?> GetStringsAsync(string langPack, string langCode, ICollection<string> keys)
    {
        if(langPack == Android && langCode == English)
        {
            Dictionary<string, string> result = new();
            foreach (var key in keys)
            {
                if (_androidLangPack.ContainsKey(key))
                {
                    result.Add(key, _androidLangPack[key]);
                }
                if (_androidLangPack.ContainsKey(key+Zero))
                {
                    result.Add(key+Zero, _androidLangPack[key+Zero]);
                }
                if (_androidLangPack.ContainsKey(key + One))
                {
                    result.Add(key + One, _androidLangPack[key + One]);
                }
                if (_androidLangPack.ContainsKey(key + Two))
                {
                    result.Add(key + Two, _androidLangPack[key + Two]);
                }
                if (_androidLangPack.ContainsKey(key + Few))
                {
                    result.Add(key + Few, _androidLangPack[key + Few]);
                }
                if (_androidLangPack.ContainsKey(key + Many))
                {
                    result.Add(key + Many, _androidLangPack[key + Many]);
                }
                if (_androidLangPack.ContainsKey(key + Other))
                {
                    result.Add(key + Other, _androidLangPack[key + Other]);
                }
            }
            return result;
        }
        
        return null;
    }
}

