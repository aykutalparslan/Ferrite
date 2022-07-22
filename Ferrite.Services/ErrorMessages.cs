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

using DotNext;

namespace Ferrite.Services;

public class ErrorMessages
{
    public static readonly ErrorMessage None = new ErrorMessage(0, string.Empty);
    public static readonly ErrorMessage PhoneNumberOccupied = new ErrorMessage(400, "PHONE_NUMBER_OCCUPIED");
    public static readonly ErrorMessage FreshChangePhoneForbidden = new ErrorMessage(406, "FRESH_CHANGE_PHONE_FORBIDDEN");
    public static readonly ErrorMessage PhoneNumberBanned = new ErrorMessage(400, "PHONE_NUMBER_BANNED");
    public static readonly ErrorMessage PhoneNumberInvalid = new ErrorMessage(406, "PHONE_NUMBER_INVALID");
    public static readonly ErrorMessage HashInvalid = new ErrorMessage(400, "HASH_INVALID");
    public static readonly ErrorMessage FreshResetAuthorizationForbidden = new ErrorMessage(406, "FRESH_RESET_AUTHORISATION_FORBIDDEN");
    public static readonly ErrorMessage UserIdInvalid = new ErrorMessage(400, "USER_ID_INVALID");
    public static readonly ErrorMessage PhotoFileMissing = new ErrorMessage(400, "PHOTO_FILE_MISSING");
    public static readonly ErrorMessage FilePartsInvalid  = new ErrorMessage(400, "FILE_PARTS_INVALID");
    public static readonly ErrorMessage PhotoFileTooBig   = new ErrorMessage(400, "PHOTO_FILE_TOO_BIG");
    public static readonly ErrorMessage PhotoFileInvalid = new ErrorMessage(400, "PHOTO_FILE_INVALID");
    public static readonly ErrorMessage FileIdInvalid = new ErrorMessage(400, "FILE_ID_INVALID");
    public static readonly ErrorMessage PeerIdInvalid = new ErrorMessage(400, "PEER_ID_INVALID");
    public static readonly ErrorMessage InvalidAuthKey = new ErrorMessage(400, "INVALID_AUTH_KEY");
}