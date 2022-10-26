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
using System.Buffers;

namespace Ferrite.Core;

public interface IFrameDecoder
{
    /// <summary>
    /// Decodes a full or partial MTProto frame
    /// </summary>
    /// <param name="bytes">Sequence of bytes to read from.</param>
    /// <param name="frame">Full or partial frame data.</param>
    /// <param name="isStream">If the frame belongs to an API method.
    /// that is an ITLStream instead of an ITLObject.</param>
    /// <returns>True if there's more data to process.</returns>
    bool Decode(ReadOnlySequence<byte> bytes, out ReadOnlySequence<byte> frame, 
        out bool isStream, out bool requiresQuickAck, out SequencePosition position);
}

