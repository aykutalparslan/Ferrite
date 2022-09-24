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

namespace Ferrite.Data.Repositories;

public interface IFileInfoRepository
{
    public bool SaveFileInfo(UploadedFileInfoDTO uploadedFile);
    public UploadedFileInfoDTO? GetFileInfo(long fileId);
    public bool PutFileInfo(UploadedFileInfoDTO uploadedFile);
    public bool PutBigFileInfo(UploadedFileInfoDTO uploadedFile);
    public UploadedFileInfoDTO? GetBigFileInfo(long fileId);
    public bool PutFilePart(FilePartDTO part);
    public bool PutBigFilePart(FilePartDTO part);
    public IReadOnlyCollection<FilePartDTO> GetFileParts(long fileId);
    public bool SaveBigFilePart(FilePartDTO part);
    public IReadOnlyCollection<FilePartDTO> GetBigFileParts(long fileId);
    public bool PutFileReference(FileReferenceDTO reference);
    public FileReferenceDTO? GetFileReference(byte[] referenceBytes);
}