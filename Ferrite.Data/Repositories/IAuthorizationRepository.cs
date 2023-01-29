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

using Ferrite.TL.slim;
using Ferrite.TL.slim.dto;

namespace Ferrite.Data.Repositories;

public interface IAuthorizationRepository
{
    public bool PutAuthorization(TLAuthInfo info);
    public TLAuthInfo? GetAuthorization(long authKeyId);
    public ValueTask<TLAuthInfo?> GetAuthorizationAsync(long authKeyId);
    public IReadOnlyList<TLAuthInfo> GetAuthorizations(string phone);
    public ValueTask<IReadOnlyList<TLAuthInfo>> GetAuthorizationsAsync(string phone);
    public bool DeleteAuthorization(long authKeyId);
    public bool PutExportedAuthorization(TLExportedAuthInfo exportedInfo);
    public TLExportedAuthInfo? GetExportedAuthorization(long userId, byte[] data);
    public ValueTask<TLExportedAuthInfo?> GetExportedAuthorizationAsync(long userId, byte[] data);
}