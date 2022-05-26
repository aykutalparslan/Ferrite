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

public enum SentCodeType
{
    None,
    /// <summary>
    /// The code was sent through the telegram app
    /// </summary>
    App,
    /// <summary>
    /// The code was sent via SMS
    /// </summary>
    Sms,
    /// <summary>
    /// The code will be sent via a phone call: a synthesized voice will tell
    /// the user which verification code to input.
    /// </summary>
    Call,
    /// <summary>
    /// The code will be sent via a flash phone call, that will be closed immediately.
    /// The phone code will then be the phone number itself, just make sure that
    /// the phone number matches the specified pattern.
    /// </summary>
    FlashCall,
    /// <summary>
    /// The code will be sent via a flash phone call, that will be closed immediately.
    /// The last digits of the phone number that calls are the code that must be entered manually by the user.
    /// </summary>
    MissedCall,
    FRESH_CHANGE_PHONE_FORBIDDEN,
    PHONE_NUMBER_BANNED,
    PHONE_NUMBER_INVALID,
    PHONE_NUMBER_OCCUPIED,
}

