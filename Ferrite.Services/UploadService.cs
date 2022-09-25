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
using Ferrite.Data.Repositories;

namespace Ferrite.Services;

public class UploadService : IUploadService
{
    private readonly IObjectStore _objectStore;
    private readonly IUnitOfWork _unitOfWork;

    public UploadService(IObjectStore objectStore, IUnitOfWork unitOfWork)
    {
        _objectStore = objectStore;
        _unitOfWork = unitOfWork;
    }
    public async Task<bool> SaveFilePart(long fileId, int filePart, Stream data)
    { 
        bool saved = await _objectStore.SaveFilePart(fileId, filePart, data);
        _unitOfWork.FileInfoRepository.PutFilePart(new FilePartDTO(fileId, filePart, (int)data.Length));
        return saved && await _unitOfWork.SaveAsync();
    }

    public async Task<bool> SaveBigFilePart(long fileId, int filePart, int fileTotalParts, Stream data)
    {
        var saved = await _objectStore.SaveBigFilePart(fileId, filePart, fileTotalParts, data);
        _unitOfWork.FileInfoRepository.PutBigFilePart(new FilePartDTO(fileId, filePart, (int)data.Length));
        return saved && await _unitOfWork.SaveAsync();
    }

    public async Task<ServiceResult<IFileOwner>> GetPhoto(long fileId, long accessHash, byte[] fileReference, 
        string thumbSize, int offset, int limit, long regMsgId, bool precise = false, bool cdnSupported = false)
    {
        var reference = _unitOfWork.FileInfoRepository.GetFileReference(fileReference);
        if (reference.IsBigfile)
        {
            var thumbs = _unitOfWork.PhotoRepository.GetThumbnails(fileId);
            long thumbFileId = 0;
            foreach (var t in thumbs)
            {
                if(t.Type == thumbSize)
                {
                    thumbFileId = t.FileId;
                    break;
                }
            }
            var file = _unitOfWork.FileInfoRepository.GetBigFileInfo(thumbFileId);
            var owner = new S3FileOwner(file, _objectStore, offset, limit, regMsgId);
            return new ServiceResult<IFileOwner>(owner, true, ErrorMessages.None);
        }
        else
        {
            var thumbs = _unitOfWork.PhotoRepository.GetThumbnails(fileId);
            long thumbFileId = 0;
            foreach (var t in thumbs)
            {
                if(t.Type == thumbSize)
                {
                    thumbFileId = t.ThumbnailFileId;
                    break;
                }
            }
            var file = _unitOfWork.FileInfoRepository.GetFileInfo(thumbFileId);
            var owner = new S3FileOwner(file, _objectStore, offset, limit, regMsgId);
            return new ServiceResult<IFileOwner>(owner, true, ErrorMessages.None);
        }
    }
}