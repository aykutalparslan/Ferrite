﻿//
//    Project Ferrite is an Implementation Telegram Server API
//    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Affero General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
namespace Ferrite.Utils;
/// <summary>
/// Helpers for getting the current UnixTime and approximate MTProto message ids
/// </summary>
public interface IMTProtoTime
{
    /// <summary>
    /// Returns a msg_id approximate 300 seconds in the past
    /// </summary>
    long FiveMinutesAgo { get; }
    /// <summary>
    /// Returns a msg_id approximate 30 seconds in the future
    /// </summary>
    long ThirtySecondsLater { get; }
    long GetUnixTimeInSeconds();
}
