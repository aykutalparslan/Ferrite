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

public class S3FileOwner : IDistributedFileOwner
{
    private readonly UploadedFileInfo _fileInfo;
    private readonly IDistributedObjectStore _objectStore;
    public S3FileOwner(UploadedFileInfo fileInfo, IDistributedObjectStore objectStore)
    {
        _fileInfo = fileInfo;
        _objectStore = objectStore;
    }
    public async Task<Stream> GetFileStream(int offset, int limit)
    {
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
                limit += (int)part.Length;
                streams.Enqueue(part);
            }
            else
            {
                var part = await _objectStore.GetFilePart(_fileInfo.Id, i);
                limit += (int)part.Length;
                streams.Enqueue(part);
            }
        }

        return new ConcatenatedStream(streams, offset, limit);
    }
}