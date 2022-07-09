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
/// Contains info about a sent verification code.
/// </summary>
public record SentCodeDTO
{
    /// <summary>
    /// Phone code type
    /// </summary>
    public SentCodeType CodeType { get; init; }
    /// <summary>
    /// Length of the code in bytes in the cases of App and SMS
    /// Length of the verification code in the cases of Call and MissedCall
    /// </summary>
    public int CodeLength { get; init; }
    /// <summary>
    /// pattern to match
    /// </summary>
    public string CodePattern { get; init; } = default!;
    /// <summary>
    /// Prefix of the phone number from which the call will be made
    /// </summary>
    public string CodePrefix { get; init; } = default!;
    /// <summary>
    /// Phone code hash, to be stored and later re-used with auth.signIn
    /// </summary>
    public string PhoneCodeHash { get; init; } = default!;
    /// <summary>
    /// Phone code type that will be sent next, if the phone code is not
    /// received within timeout seconds: to send it use auth.resendCode
    /// </summary>
    public SentCodeType NextType { get; init; }
    /// <summary>
    /// Timeout for reception of the phone code
    /// </summary>
    public int Timeout { get; init; }
}

