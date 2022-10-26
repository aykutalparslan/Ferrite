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

using DotNext.Buffers;
using Ferrite.Transport;

namespace Ferrite.Core.Features;

public class TransportErrorFeature : ITransportErrorFeature
{
    public void SendTransportError(int errorCode, SparseBufferWriter<byte> writer,
        IFrameEncoder encoder, WebSocketHandler? webSocketHandler,
        MTProtoConnection connection)
    {
        writer.Clear();
        writer.WriteInt32(-1 * errorCode, true);
        var message = writer.ToReadOnlySequence();
        var encoded = encoder.Encode(message);
        if (webSocketHandler != null)
        {
            webSocketHandler.WriteHeaderTo(connection.TransportConnection.Transport.Output, encoded.Length);
        }

        connection.TransportConnection.Transport.Output.Write(encoded);
        connection.TransportConnection.Transport.Output.FlushAsync();
    }
}