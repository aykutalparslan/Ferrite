//
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
using System;
namespace Ferrite.Core;

/// <summary>
/// Calculates msg_id values 30 seconds in the future and
/// 300 seconds in the past periodically.
/// </summary>
public class MTProtoTime : IMTProtoTime
{
    //private static Lazy<MTProtoTime> _instance = new Lazy<MTProtoTime>(
    //    () => new MTProtoTime(),
    //    LazyThreadSafetyMode.ExecutionAndPublication);

    private long _seconds;
    private long _fiveMinutesAgo;
    /// <summary>
    /// Returns a msg_id approximate 300 seconds in the past
    /// </summary>
    public long FiveMinutesAgo => _fiveMinutesAgo;
    private long _thirtySecondsLater;
    /// <summary>
    /// Returns a msg_id approximate 30 seconds in the future
    /// </summary>
    public long ThirtySecondsLater => _thirtySecondsLater;
    private readonly Task _keepTimeTask;
    private async Task KeepTime()
    {
        while (true)
        {
            _seconds = DateTimeOffset.Now.ToUnixTimeSeconds();
            _fiveMinutesAgo = (_seconds - 300) * 4294967296L;
            _thirtySecondsLater = (_seconds + 30) * 4294967296L;
            await Task.Delay(1000);
        }
    }
    /// <summary>
    /// Fully thread safe; uses locking to ensure that only one thread initializes the value.
    /// </summary>
    //public static MTProtoTime Instance => _instance.Value;
    public MTProtoTime()
    {
        _keepTimeTask = KeepTime();
    }
}

