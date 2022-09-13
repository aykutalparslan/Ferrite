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

using MessagePack;

namespace Ferrite.Data.Repositories;

public class FileInfoRepository : IFileInfoRepository
{
    private readonly IKVStore _storeFiles;
    private readonly IKVStore _storeBigFiles;
    private readonly IKVStore _storeReferences;
    private readonly IKVStore _storeFileParts;
    private readonly IKVStore _storeBigFileParts;
    public FileInfoRepository(IKVStore storeFiles, IKVStore storeBigFiles,
        IKVStore storeReferences,IKVStore storeFileParts, IKVStore storeBigFileParts)
    {
        _storeFiles = storeFiles;
        _storeFiles.SetSchema(new TableDefinition("ferrite", "files",
            new KeyDefinition("pk",
                new DataColumn { Name = "file_id", Type = DataType.Long })));
        _storeBigFiles = storeBigFiles;
        _storeBigFiles.SetSchema(new TableDefinition("ferrite", "big_files",
            new KeyDefinition("pk",
                new DataColumn { Name = "file_id", Type = DataType.Long })));
        _storeReferences = storeReferences;
        _storeReferences.SetSchema(new TableDefinition("ferrite", "file_references",
            new KeyDefinition("pk",
                new DataColumn { Name = "reference_bytes", Type = DataType.Bytes })));
        _storeFileParts = storeFileParts;
        _storeFileParts.SetSchema(new TableDefinition("ferrite", "file_parts",
            new KeyDefinition("pk",
                new DataColumn { Name = "file_id", Type = DataType.Long },
                new DataColumn { Name = "file_part", Type = DataType.Int })));
        _storeBigFileParts = storeFileParts;
        _storeBigFileParts.SetSchema(new TableDefinition("ferrite", "big_file_parts",
            new KeyDefinition("pk",
                new DataColumn { Name = "file_id", Type = DataType.Long },
                new DataColumn { Name = "file_part", Type = DataType.Int })));
    }
    public bool SaveFileInfo(UploadedFileInfoDTO uploadedFile)
    {
        var infoBytes = MessagePackSerializer.Serialize(uploadedFile);
        return _storeFiles.Put(infoBytes, uploadedFile.Id);
    }

    public UploadedFileInfoDTO? GetFileInfo(long fileId)
    {
        var infoBytes = _storeFiles.Get(fileId);
        if (infoBytes == null) return null;
        var info = MessagePackSerializer.Deserialize<UploadedFileInfoDTO>(infoBytes);
        return info;
    }

    public bool PutBigFileInfo(UploadedFileInfoDTO uploadedFile)
    {
        var infoBytes = MessagePackSerializer.Serialize(uploadedFile);
        return _storeBigFiles.Put(infoBytes, uploadedFile.Id);
    }

    public UploadedFileInfoDTO? GetBigFileInfo(long fileId)
    {
        var infoBytes = _storeBigFiles.Get(fileId);
        if (infoBytes == null) return null;
        var info = MessagePackSerializer.Deserialize<UploadedFileInfoDTO>(infoBytes);
        return info;
    }

    public bool PutFilePart(FilePartDTO part)
    {
        var partBytes = MessagePackSerializer.Serialize(part);
        return _storeFileParts.Put(partBytes, part.FileId, part.PartNum);
    }

    public IReadOnlyCollection<FilePartDTO> GetFileParts(long fileId)
    {
        List<FilePartDTO> parts = new();
        var iter = _storeFileParts.Iterate(fileId);
        foreach (var partBytes in iter)
        {
            var part = MessagePackSerializer.Deserialize<FilePartDTO>(partBytes);
            parts.Add(part);
        }

        return parts;
    }

    public bool SaveBigFilePart(FilePartDTO part)
    {
        var partBytes = MessagePackSerializer.Serialize(part);
        return _storeBigFileParts.Put(partBytes, part.FileId, part.PartNum);
    }

    public IReadOnlyCollection<FilePartDTO> GetBigFileParts(long fileId)
    {
        List<FilePartDTO> parts = new();
        var iter = _storeBigFileParts.Iterate(fileId);
        foreach (var partBytes in iter)
        {
            var part = MessagePackSerializer.Deserialize<FilePartDTO>(partBytes);
            parts.Add(part);
        }

        return parts;
    }

    public bool PutFileReference(FileReferenceDTO reference)
    {
        var referenceBytes = MessagePackSerializer.Serialize(reference);
        return _storeReferences.Put(referenceBytes, reference.ReferenceBytes);
    }

    public FileReferenceDTO? GetFileReference(byte[] referenceBytes)
    {
        var reference = _storeReferences.Get(referenceBytes);
        if (reference == null) return null;
        var info = MessagePackSerializer.Deserialize<FileReferenceDTO>(reference);
        return info;
    }
}