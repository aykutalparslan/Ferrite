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

using Ferrite.Core.Features;
using Ferrite.Transport;

namespace Ferrite.Core;

public class TransportControllerFactory
{
    private readonly IQuickAckFeature _quickAck;
    private readonly ITransportErrorFeature _transportError;
    private readonly INotifySessionCreatedFeature _notifySessionCreated;
    private readonly IWebSocketFeature _webSocket;

    public TransportControllerFactory(IQuickAckFeature quickAck,
        ITransportErrorFeature transportError,
        INotifySessionCreatedFeature notifySessionCreated)
    {
        _quickAck = quickAck;
        _transportError = transportError;
        _notifySessionCreated = notifySessionCreated;
    }

    public TransportController Create(ITransportConnection connection)
    {
        return new TransportController(_quickAck, _transportError, _notifySessionCreated,
            new WebSocketFeature(connection));
    }
}