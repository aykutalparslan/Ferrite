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
using System.Buffers.Binary;
using DotNext.Buffers;
using Ferrite.Services;

namespace Ferrite.Core.Connection.TransportFeatures;

public class QuickAckFeature : IQuickAckFeature
{
    public ReadOnlySequence<byte> GenerateQuickAck(int ack, MTProtoTransport transport)
    {
        BufferWriterSlim<byte> writer = new(stackalloc byte[4]);
        writer.Clear();
        ack |= 1 << 31;
        if (transport == MTProtoTransport.Abridged)
        {
            ack = BinaryPrimitives.ReverseEndianness(ack);
        }
        writer.WriteInt32(ack, true);
        var msg = writer.WrittenSpan;
        return new ReadOnlySequence<byte>(writer.WrittenSpan.ToArray());
    }
}