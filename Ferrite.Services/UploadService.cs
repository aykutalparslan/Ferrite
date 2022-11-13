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
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Data.Repositories;
using Ferrite.TL.slim.layer146.storage;
using Ferrite.TL.slim.layer146.upload;
using Ferrite.TL.slim.mtproto;

namespace Ferrite.Services;

public class UploadService : IUploadService
{
    private readonly IObjectStore _objectStore;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRandomGenerator _random;

    public UploadService(IObjectStore objectStore, IUnitOfWork unitOfWork, 
        IRandomGenerator random)
    {
        _objectStore = objectStore;
        _unitOfWork = unitOfWork;
        _random = random;
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

    public async Task<ServiceResult<UploadedFileInfoDTO>> SaveFile(InputFileDTO file)
    {
        UploadedFileInfoDTO? info;
        int size = 0;
        IReadOnlyCollection<FilePartDTO> fileParts;
        if (file != null)
        {
            fileParts = _unitOfWork.FileInfoRepository.GetFileParts(file.Id);
            if (fileParts.Count != file.Parts ||
                fileParts.First().PartNum != 0 ||
                fileParts.Last().PartNum != file.Parts - 1)
            {
                return new ServiceResult<UploadedFileInfoDTO>(null, false, ErrorMessages.FilePartsInvalid);
            }
            foreach (var part in fileParts)
            {
                size += part.PartSize;
            }
            if (size > 5242880)
            {
                return new ServiceResult<UploadedFileInfoDTO>(null, false, ErrorMessages.PhotoFileTooBig);
            }

            var accessHash = _random.NextLong();
            byte[] reference = _random.GetRandomBytes(16);
            info = new UploadedFileInfoDTO(file.Id, fileParts.First().PartSize, file.Parts,
                accessHash, file.Name, file.MD5Checksum, DateTimeOffset.Now, file.IsBigfile, reference);
        } 
        else
        {
            return new ServiceResult<UploadedFileInfoDTO>(null, false, ErrorMessages.PhotoFileMissing);
        }
        if (info.IsBigFile)
        {
            _unitOfWork.FileInfoRepository.PutBigFileInfo(info);
        }
        else
        {
            _unitOfWork.FileInfoRepository.PutFileInfo(info);
        }
        
        _unitOfWork.FileInfoRepository.PutFileReference(new FileReferenceDTO(info.FileReference, info.Id, info.IsBigFile));
        await _unitOfWork.SaveAsync();
        return new ServiceResult<UploadedFileInfoDTO>(info, true, ErrorMessages.None);
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
            var owner = _objectStore.GetFileOwner(file, offset, limit, regMsgId, 
                GenerateStreamHeader(regMsgId, StreamFileType.Jpeg));
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
            var owner = _objectStore.GetFileOwner(file, offset, limit, regMsgId, 
                GenerateStreamHeader(regMsgId, StreamFileType.Jpeg));
            return new ServiceResult<IFileOwner>(owner, true, ErrorMessages.None);
        }
    }
    private static byte[] GenerateStreamHeader(long reqMsgId, StreamFileType fileType)
    {
        var file = File_.Builder()
            .with_type(GetFileType(fileType))
            .with_mtime((int)DateTimeOffset.Now.ToUnixTimeSeconds())
            .Build();
        var rpcResult = RpcResult.Builder()
            .with_req_msg_id(reqMsgId)
            .with_result(file.ToReadOnlySpan())
            .Build();
        byte[] resultHeader = new byte[24];
        rpcResult.ToReadOnlySpan()[..24].CopyTo(resultHeader);
        
        return resultHeader;
    }

    private static ReadOnlySpan<byte> GetFileType(StreamFileType fileType) => fileType switch
    {
        StreamFileType.Gif => new FileGif().ToReadOnlySpan(),
        StreamFileType.Jpeg => new FileJpeg().ToReadOnlySpan(),
        StreamFileType.Mov => new FileMov().ToReadOnlySpan(),
        StreamFileType.Mp3 => new FileMp3().ToReadOnlySpan(),
        StreamFileType.Mp4 => new FileMp4().ToReadOnlySpan(),
        StreamFileType.Partial => new FilePartial().ToReadOnlySpan(),
        StreamFileType.Png => new FilePng().ToReadOnlySpan(),
        StreamFileType.Webp => new FileWebp().ToReadOnlySpan(),
        _ => new FileUnknown().ToReadOnlySpan(),
    };
}