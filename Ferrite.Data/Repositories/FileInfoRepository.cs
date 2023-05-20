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

using Ferrite.TL.slim.baseLayer.dto;
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
        _storeBigFileParts = storeBigFileParts;
        _storeBigFileParts.SetSchema(new TableDefinition("ferrite", "big_file_parts",
            new KeyDefinition("pk",
                new DataColumn { Name = "file_id", Type = DataType.Long },
                new DataColumn { Name = "file_part", Type = DataType.Int })));
    }

    public TLUploadedFileInfo? GetFileInfo(long fileId)
    {
        var infoBytes = _storeFiles.Get(fileId);
        if (infoBytes == null) return null;
        return new TLUploadedFileInfo(infoBytes, 0, infoBytes.Length);
    }

    public bool PutFileInfo(TLUploadedFileInfo uploadedFile)
    {
        var infoBytes = uploadedFile.AsSpan().ToArray();
        return _storeFiles.Put(infoBytes, uploadedFile.AsUploadedFileInfo().Id);
    }

    public bool PutBigFileInfo(TLUploadedFileInfo uploadedFile)
    {
        var infoBytes = uploadedFile.AsSpan().ToArray();
        return _storeBigFiles.Put(infoBytes, uploadedFile.AsUploadedFileInfo().Id);
    }

    public TLUploadedFileInfo? GetBigFileInfo(long fileId)
    {
        var infoBytes = _storeBigFiles.Get(fileId);
        if (infoBytes == null) return null;
        return new TLUploadedFileInfo(infoBytes, 0, infoBytes.Length);
    }

    public bool PutFilePart(TLFilePart part)
    {
        var partBytes = part.AsSpan().ToArray();
        return _storeFileParts.Put(partBytes, part.AsFilePart().FileId, part.AsFilePart().PartNum);
    }

    public bool PutBigFilePart(TLFilePart part)
    {
        var partBytes = part.AsSpan().ToArray();
        return _storeBigFileParts.Put(partBytes, part.AsFilePart().FileId, part.AsFilePart().PartNum);
    }

    public IReadOnlyCollection<TLFilePart> GetFileParts(long fileId)
    {
        List<TLFilePart> parts = new();
        var iter = _storeFileParts.Iterate(fileId);
        foreach (var partBytes in iter)
        {
            var part = new TLFilePart(partBytes, 0, partBytes.Length);
            parts.Add(part);
        }

        return parts;
    }

    public bool SaveBigFilePart(TLFilePart part)
    {
        var partBytes = part.AsSpan().ToArray();
        return _storeBigFileParts.Put(partBytes, part.AsFilePart().FileId, part.AsFilePart().PartNum);
    }

    public IReadOnlyCollection<TLFilePart> GetBigFileParts(long fileId)
    {
        List<TLFilePart> parts = new();
        var iter = _storeBigFileParts.Iterate(fileId);
        foreach (var partBytes in iter)
        {
            var part = new TLFilePart(partBytes, 0, partBytes.Length);
            parts.Add(part);
        }

        return parts;
    }

    public bool PutFileReference(TLFileReference reference)
    {
        var referenceBytes = reference.AsSpan().ToArray();
        return _storeReferences.Put(referenceBytes, 
            reference.AsFileReference().ReferenceBytes.ToArray());
    }

    public TLFileReference? GetFileReference(byte[] referenceBytes)
    {
        var reference = _storeReferences.Get(referenceBytes);
        if (reference == null) return null;
        return new TLFileReference(reference, 0 , reference.Length);
    }
}