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

public interface ILangPackRepository
{
    public bool SaveLanguage(string langPack, LangPackLanguageDTO languageDto);
    public bool SaveLangPackDifference(string langPack, LangPackDifferenceDTO difference);
    public ValueTask<List<LangPackLanguageDTO>> GetLanguagesAsync(string? langPack);
    public ValueTask<LangPackLanguageDTO?> GetLanguageAsync(string langPack, string langCode);
    public ValueTask<LangPackDifferenceDTO?> GetLangPackAsync(string langPack, string langCode);
    public ValueTask<LangPackDifferenceDTO?> GetDifferenceAsync(string langPack, string langCode, int fromVersion);
    public ValueTask<List<LangPackStringDTO>> GetStringsAsync(string langPack, string langCode,
        ICollection<string> keys);
}