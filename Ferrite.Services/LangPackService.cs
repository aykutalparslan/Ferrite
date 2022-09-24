//
//  Project Ferrite is an Implementation of the Telegram Server API
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
using Ferrite.Data.Repositories;

namespace Ferrite.Services;

public class LangPackService : ILangPackService
{
    private readonly IUnitOfWork _unitOfWork;
    public LangPackService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<LangPackDifferenceDTO?> GetDifferenceAsync(string langPack, string langCode, int fromVersion)
    {
        return await _unitOfWork.LangPackRepository.GetDifferenceAsync(langPack, langCode, fromVersion);
    }

    public async Task<LangPackDifferenceDTO?> GetLangPackAsync(string langPack, string langCode)
    {
        return await _unitOfWork.LangPackRepository.GetLangPackAsync(langPack, langCode);
    }

    public async Task<LangPackLanguageDTO?> GetLanguageAsync(string langPack, string langCode)
    {
        return await _unitOfWork.LangPackRepository.GetLanguageAsync(langPack, langCode);
    }

    public async Task<ICollection<LangPackLanguageDTO>> GetLanguagesAsync(string langPack)
    {
        return await _unitOfWork.LangPackRepository.GetLanguagesAsync(langPack);
    }

    public async Task<ICollection<LangPackStringDTO>> GetStringsAsync(string langPack, string langCode,
        ICollection<string> keys)
    {
        return await _unitOfWork.LangPackRepository.GetStringsAsync(langPack, langCode, keys);
    }
}

