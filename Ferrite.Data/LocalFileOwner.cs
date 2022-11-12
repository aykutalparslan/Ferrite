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

public class LocalFileOwner : IFileOwner
{
    private readonly UploadedFileInfoDTO _fileInfo;
    private readonly IObjectStore _objectStore;
    private readonly int _offset;
    private readonly int _limit;

    public LocalFileOwner(UploadedFileInfoDTO fileInfo, IObjectStore objectStore, 
        int offset, int limit, long reqMsgId, byte[] streamHeader)
    {
        TLObjectHeader = streamHeader;
        _fileInfo = fileInfo;
        _objectStore = objectStore;
        _offset = offset;
        _limit = limit;
        ReqMsgId = reqMsgId;
    }

    public byte[] TLObjectHeader { get; init; }

    public async ValueTask<Stream> GetFileStream()
    {
        int offset = _offset;
        Queue<Stream> streams = new Queue<Stream>();
        for (int i = 0; i < _fileInfo.Parts; i++)
        {
            if (offset >= _fileInfo.PartSize)
            {
                offset -= _fileInfo.PartSize;
                continue;
            }
            if (_fileInfo.IsBigFile)
            {
                var part = await _objectStore.GetBigFilePart(_fileInfo.Id, i);
                streams.Enqueue(part);
            }
            else
            {
                var part = await _objectStore.GetFilePart(_fileInfo.Id, i);
                streams.Enqueue(part);
            }
        }

        return new ConcatenatedStream(streams, offset, _limit);
    }

    public long ReqMsgId { get; }
}