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

namespace Ferrite.Data;

public interface ILangPackDataStore
{
    public Task<bool> SaveLanguageAsync(LangPackLanguage language);
    public Task<ICollection<LangPackLanguage>> GetLanguagesAsync(string? langPack);
    public Task<LangPackLanguage?> GetLanguagesAsync(string langPack, string langCode);
    public Task<LangPackDifference?> GetLangPackAsync(string langPack, string langCode);
    public Task<LangPackDifference?> GetDifferenceAsync(string langPack, string langCode, int fromVersion);
    public Task<ICollection<LangPackString>> GetStringsAsync(string langPack, string langCode, ICollection<string> keys);
}