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

using System.Buffers;
using Ferrite.Crypto;
using Ferrite.Services;

namespace Ferrite.Core.Framing;

public class PaddedIntermediateFrameDecoder : FrameDecoderBase
{
 protected override bool DecodeLength(ref SequenceReader<byte> reader, out bool emptyFrame)
 {
  if (reader.Remaining < 4)
  {
   emptyFrame = true;
   return false;
  }

  reader.TryCopyTo(LengthBytes);
  Decryptor?.Transform(LengthBytes);
  var requiresQuickAck = CheckRequiresQuickAck(LengthBytes, 3);

  Length = (LengthBytes[0]) |
           (LengthBytes[1] << 8) |
           (LengthBytes[2] << 16) |
           (LengthBytes[3] << 24);
  reader.Advance(4);
  emptyFrame = false;
  return requiresQuickAck;
 }

 public PaddedIntermediateFrameDecoder(IMTProtoService mtproto) : base(mtproto)
 {
 }

 public PaddedIntermediateFrameDecoder(Aes256Ctr decryptor, IMTProtoService mtproto) : base(decryptor, mtproto)
 {
 }
}

