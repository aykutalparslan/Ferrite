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
namespace Ferrite.Data.Auth;
/// <summary>
/// Settings used by telegram servers for sending the confirm code.
/// </summary>
public record CodeSettingsDTO
{
    /// <summary>
    /// Whether to allow phone verification via phone calls.
    /// </summary>
    public bool AllowFlashcall { get; init; }
    /// <summary>
    /// Pass true if the phone number is used on the current device.
    /// Ignored if allow_flashcall is not set.
    /// </summary>
    public bool CurrentNumber { get; init; }
    /// <summary>
    /// If a token that will be included in eventually sent SMSs is required:
    /// required in newer versions of android, to use the
    /// <see href="https://developers.google.com/identity/sms-retriever/overview">android SMS receiver APIs</see>
    /// </summary>
    public bool AllowAppHash { get; init; }
    /// <summary>
    /// Whether this device supports receiving the code using
    /// the auth.codeTypeMissedCall method
    /// </summary>
    public bool AllowMissedCall { get; init; }
    /// <summary>
    /// Previously stored logout tokens,
    /// <see href="https://core.telegram.org/api/auth#logout-tokens">see the documentation for more info »</see>
    /// </summary>
    public ICollection<byte[]> LogoutTokens { get; init; } = default!;
}

