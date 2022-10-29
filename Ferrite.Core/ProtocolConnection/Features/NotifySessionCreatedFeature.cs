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
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.mtproto;

namespace Ferrite.Core.Features;

public class NotifySessionCreatedFeature : INotifySessionCreatedFeature
{
    public void Notify(ITLObjectFactory factory,
        MTProtoConnection connection, MTProtoSession session,
        long firstMessageId, long serverSalt)
    {
        var newSessionCreated = factory.Resolve<NewSessionCreated>();
        newSessionCreated.FirstMsgId = firstMessageId;
        newSessionCreated.ServerSalt = serverSalt;
        newSessionCreated.UniqueId = session.UniqueSessionId;
        MTProtoMessage newSessionMessage = new();
        newSessionMessage.Data = newSessionCreated.TLBytes.ToArray();
        newSessionMessage.IsContentRelated = false;
        newSessionMessage.IsResponse = false;
        newSessionMessage.SessionId = session.SessionId;
        newSessionMessage.MessageType = MTProtoMessageType.NewSession;
        _ = connection.SendAsync(newSessionMessage);
    }
}