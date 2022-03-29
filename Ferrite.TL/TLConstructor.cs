/*
 *   Project Ferrite is an Implementation Telegram Server API
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
public enum TLConstructor
{
    Vector = 481674261,
    ResPQ = 85337187,
    PQInnerData = -2083955988,
    PQInnerDataDc = -1443537003,
    PQInnerDataTempDc = 1459478408,
    ServerDhParamsOk = -790100132,
    ServerDhInnerData = -1249309254,
    ClientDhInnerData = 1715713620,
    DhGenOk = 1003222836,
    DhGenRetry = 1188831161,
    DhGenFail = -1499615742,
    BindAuthKeyInner = 1973679973,
    RpcResult = -212046591,
    RpcError = 558156313,
    RpcAnswerUnknown = 1579864942,
    RpcAnswerDroppedRunning = -847714938,
    RpcAnswerDropped = -1539647305,
    FutureSalt = 155834844,
    FutureSalts = -1370486635,
    Pong = 880243653,
    DestroySessionOk = -501201412,
    DestroySessionNone = 1658015945,
    NewSessionCreated = -1631450872,
    MsgContainer = 1945237724,
    Message = 1538843921,
    MsgCopy = -530561358,
    GzipPacked = 812830625,
    MsgsAck = 1658238041,
    BadMsgNotification = -1477445615,
    BadServerSalt = -307542917,
    MsgResendReq = 2105940488,
    MsgsStateReq = -630588590,
    MsgsStateInfo = 81704317,
    MsgsAllInfo = -1933520591,
    MsgDetailedInfo = 661470918,
    MsgNewDetailedInfo = -2137147681,
    DestroyAuthKeyOk = -161422892,
    DestroyAuthKeyNone = 178201177,
    DestroyAuthKeyFail = -368010477,
    ReqPqMulti = -1099002127,
    ReqDhParams = -686627650,
    SetClientDhParams = -184262881,
    RpcDropAnswer = 1491380032,
    GetFutureSalts = -1188971260,
    Ping = 2059302892,
    PingDelayDisconnect = -213746804,
    DestroySession = -414113498,
    HttpWait = -1835453025,
    DestroyAuthKey = -784117408
}