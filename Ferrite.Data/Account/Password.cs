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

namespace Ferrite.Data.Account;

public record Password
{
    public bool HasRecovery { get; init; }
    public bool HasSecureValues { get; init; }
    public bool HasPassword { get; init; }
    public PasswordKdfAlgo? CurrentAlgo { get; init; }
    public byte[]? SrpB { get; init; }
    public long? SrpId { get; init; }
    public string? EmailUnconfirmedPattern { get; init; }
    public PasswordKdfAlgo NewAlgo { get; init; } = default!;
    public SecurePasswordKdfAlgo NewSecureAlgo { get; init; } = default!;
    public byte[] SecureRandom { get; init; } = default!;
    public int? PendingResetDate { get; init; }
}