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

using System.Buffers;
using System.Net;
using Ferrite.Services;
using Ferrite.Transport;

namespace Ferrite.Core;

public interface IUnencryptedMessageHandler
{
    public void HandleIncomingMessage(in ReadOnlySequence<byte> bytes, 
        MTProtoConnection connection,
        MTProtoSession session);

    public void HandleOutgoingMessage(MTProtoMessage message, MTProtoConnection connection,
        MTProtoSession session, IFrameEncoder encoder, WebSocketHandler? webSocketHandler);
}