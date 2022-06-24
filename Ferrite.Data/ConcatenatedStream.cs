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

namespace Ferrite.Data;

public class ConcatenatedStream: Stream
{
    private int _position;
    private readonly Queue<Stream> _streams;
    private Stream? _currentStream;
    private int _currentStreamPosition;
    private int _offset;
    private readonly int _limit;
    public ConcatenatedStream(Queue<Stream> streams, int offset, int limit)
    {
        _streams = streams;
        foreach (var stream in streams)
        {
            Length += stream.Length;
        }
        _offset = offset;
        _limit = limit;
        Length = Math.Min(Length - _offset, _limit);
    }
    public override void Flush()
    {
        throw new NotImplementedException();
    }
    public override int Read(byte[] buffer, int offset, int count)
    {
        SetCurrentStream();
        Discard();
        if (_currentStream == null)
        {
            return 0;
        }
        int toBeCopied = (int)Math.Min(count, _currentStream.Length - _currentStreamPosition);
        toBeCopied = Math.Min(toBeCopied, _limit - _position);
        if (toBeCopied == 0)
        {
            return 0;
        }
        _currentStream.Read(buffer, offset, toBeCopied);
        _currentStreamPosition += toBeCopied;
        _position += toBeCopied;
        if (_streams.Count == 0 &&
            _currentStreamPosition == _currentStream.Length)
        {
            _currentStream.Dispose();
            _currentStream = null;
        }

        return toBeCopied;
    }

    private void Discard()
    {
        while(_offset > 0)
        {
            int toBeDiscarded = (int)Math.Min(_offset, _currentStream.Length - _currentStreamPosition);
            var discard = new byte[toBeDiscarded];
            _currentStream.Read(discard, 0, toBeDiscarded);
            _currentStreamPosition += toBeDiscarded;
            _offset -= toBeDiscarded;
            SetCurrentStream();
        }
    }

    private void SetCurrentStream()
    {
        if (_currentStream == null && _streams.Count > 0)
        {
            _currentStream = _streams.Dequeue();
            _currentStreamPosition = 0;
        }
        else if (_currentStream != null && 
                 _currentStreamPosition == _currentStream.Length)
        {
            _currentStream.Dispose();
            _currentStream = null;
            _currentStreamPosition = 0;
            if (_streams.Count > 0)
            {
                _currentStream = _streams.Dequeue();
            }
        }
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
    public override long Position { get => _position; 
        set => throw new NotSupportedException(); }

    public override void Close()
    {
        if (_currentStream != null)
        {
            _currentStream.Dispose();
        }
        for (int i = 0; i < _streams.Count; i++)
        {
            var stream = _streams.Dequeue();
            stream.Dispose();
        }
        base.Close();
    }
}