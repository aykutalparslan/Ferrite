/*
 *   Project Ferrite is an Implementation of the Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;

namespace Ferrite.TL;
public class TLConstructor
{
    public const int Vector = 481674261;
    public const int ResPQ = 85337187;
    public const int PQInnerData = -2083955988;
    public const int PQInnerDataDc = -1443537003;
    public const int PQInnerDataTempDc = 1459478408;
    public const int ServerDhParamsOk = -790100132;
    public const int ServerDhInnerData = -1249309254;
    public const int ClientDhInnerData = 1715713620;
    public const int DhGenOk = 1003222836;
    public const int DhGenRetry = 1188831161;
    public const int DhGenFail = -1499615742;
    public const int BindAuthKeyInner = 1973679973;
    public const int RpcResult = -212046591;
    public const int RpcError = 558156313;
    public const int RpcAnswerUnknown = 1579864942;
    public const int RpcAnswerDroppedRunning = -847714938;
    public const int RpcAnswerDropped = -1539647305;
    public const int FutureSalt = 155834844;
    public const int FutureSalts = -1370486635;
    public const int Pong = 880243653;
    public const int DestroySessionOk = -501201412;
    public const int DestroySessionNone = 1658015945;
    public const int NewSessionCreated = -1631450872;
    public const int MsgContainer = 1945237724;
    public const int Message = 1538843921;
    public const int MsgCopy = -530561358;
    public const int GzipPacked = 812830625;
    public const int MsgsAck = 1658238041;
    public const int BadMsgNotification = -1477445615;
    public const int BadServerSalt = -307542917;
    public const int MsgResendReq = 2105940488;
    public const int MsgsStateReq = -630588590;
    public const int MsgsStateInfo = 81704317;
    public const int MsgsAllInfo = -1933520591;
    public const int MsgDetailedInfo = 661470918;
    public const int MsgNewDetailedInfo = -2137147681;
    public const int DestroyAuthKeyOk = -161422892;
    public const int DestroyAuthKeyNone = 178201177;
    public const int DestroyAuthKeyFail = -368010477;
    public static readonly int ReqPqMulti = -1099002127;
    public static readonly int ReqDhParams = -686627650;
    public static readonly int SetClientDhParams = -184262881;
    public static readonly int RpcDropAnswer = 1491380032;
    public static readonly int GetFutureSalts = -1188971260;
    public static readonly int Ping = 2059302892;
    public static readonly int PingDelayDisconnect = -213746804;
    public static readonly int DestroySession = -414113498;
    public static readonly int HttpWait = -1835453025;
    public static readonly int DestroyAuthKey = -784117408;
}