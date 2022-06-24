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
using System.IO.Pipelines;
using DotNext.Buffers;

namespace Ferrite.TL;

public class MTProtoStream : Stream
{
    private Stream _pipeStream;
    private int _remaining;
    public MTProtoStream(PipeReader reader, int count)
    {
        _pipeStream = reader.AsStream();
        _remaining = count;
        Length = count;
    }
    public override void Flush()
    {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        int toBeCopied = Math.Min(_remaining, count);
        if (toBeCopied == 0)
        {
            return 0;
        }
        int copied = _pipeStream.Read(buffer, offset, toBeCopied);
        _remaining -= copied;
        return copied;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => false;
    public override long Length { get; }
    public override long Position { get => 0; 
        set => throw new NotSupportedException(); }

    public override void Close()
    {
        _pipeStream.Close();
        base.Close();
    }
}