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

using System.IO.Pipelines;
using Ferrite.Data;

namespace Ferrite.Services;

public class UploadService : IUploadService
{
    private readonly IDistributedObjectStore _objectStore;
    private readonly IPersistentStore _store;
    public UploadService(IDistributedObjectStore objectStore, IPersistentStore store)
    {
        _objectStore = objectStore;
        _store = store;
    }
    public async Task<bool> SaveFilePart(long fileId, int filePart, Stream data)
    {
        return await _objectStore.SaveFilePart(fileId, filePart, data);
    }

    public async Task<bool> SaveBigFilePart(long fileId, int filePart, int fileTotalParts, Stream data)
    {
        return await _objectStore.SaveBigFilePart(fileId, filePart, fileTotalParts, data);
    }

    public async Task<ServiceResult<IDistributedFileOwner>> GetPhoto(long fileId, long accessHash, byte[] fileReference, 
        string thumbSize, int offset, int limit, bool precise = false, bool cdnSupported = false)
    {
        var reference = await _store.GetFileReferenceAsync(fileReference);
        if (reference.IsBigfile)
        {
            var file = await _store.GetBigFileInfoAsync(fileId);
            if (file.AccessHash != accessHash)
            {
                return new ServiceResult<IDistributedFileOwner>(null, false, ErrorMessages.FileIdInvalid);
            }
            var owner = new S3FileOwner(file, _objectStore, offset, limit);
            return new ServiceResult<IDistributedFileOwner>(owner, true, ErrorMessages.None);
        }
        else
        {
            var file = await _store.GetFileInfoAsync(fileId);
            if (file.AccessHash != accessHash)
            {
                return new ServiceResult<IDistributedFileOwner>(null, false, ErrorMessages.FileIdInvalid);
            }
            var owner = new S3FileOwner(file, _objectStore, offset, limit);
            return new ServiceResult<IDistributedFileOwner>(owner, true, ErrorMessages.None);
        }
    }
}