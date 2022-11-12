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

using FASTER.core;

namespace Ferrite.Data;

public class LocalObjectStore : IObjectStore
{
    private readonly string _parentDir;
    private readonly string _smallFilesDir;
    private readonly string _bigFilesDir;
    private FasterContext<ObjectId, ObjectMetadata> _metadataStore;
    private readonly ClientSession<ObjectId, ObjectMetadata, ObjectMetadata, ObjectMetadata, Empty, 
        IFunctions<ObjectId, ObjectMetadata, ObjectMetadata, ObjectMetadata, Empty>> _session;

    public LocalObjectStore(string path)
    {
        _parentDir = path;
        _smallFilesDir = Path.Combine(_parentDir, "small");
        _bigFilesDir = Path.Combine(_parentDir, "big");
        _metadataStore = new FasterContext<ObjectId, ObjectMetadata>(path+"-faster-object-metadata");
        _session = _metadataStore.Store.NewSession(new SimpleFunctions<ObjectId, ObjectMetadata>());
        if (!Directory.Exists(_parentDir)) Directory.CreateDirectory(_parentDir);
        if (!Directory.Exists(_smallFilesDir)) Directory.CreateDirectory(_smallFilesDir);
        if (!Directory.Exists(_bigFilesDir)) Directory.CreateDirectory(_bigFilesDir);
    }
    public async ValueTask<bool> SaveFilePart(long fileId, int filePart, Stream data)
    {
        ObjectId key = new (fileId, filePart);
        ObjectMetadata metadata = new (fileId, filePart, 
            (int)data.Length, DateTimeOffset.Now, false);
        return await SaveFile(data, key, metadata);
    }

    private async Task<bool> SaveFile(Stream data, ObjectId key, ObjectMetadata metadata)
    {
        _session.Upsert(key, metadata);
        var filePath = GetFilePath(metadata);
        if (File.Exists(filePath)) File.Delete(filePath);
        await using var fileStream = File.Create(filePath);
        await data.CopyToAsync(fileStream);
        fileStream.Close();
        await _session.WaitForCommitAsync();
        return true;
    }

    private string GetFilePath(ObjectMetadata metadata)
    {
        string folderName = Path.Combine(metadata.IsBig ? _bigFilesDir : _smallFilesDir,
            metadata.Timestamp.Year +
            metadata.Timestamp.Month.ToString("00") + metadata.Timestamp.Day.ToString("00"));
        if (!Directory.Exists(folderName)) Directory.CreateDirectory(folderName);
        string fileName = metadata.FileId.ToString("X") + "-" + metadata.PartNum.ToString("X");
        var filePath = Path.Combine(folderName, fileName);
        return filePath;
    }

    public async ValueTask<bool> SaveBigFilePart(long fileId, int filePart, int fileTotalParts, Stream data)
    {
        ObjectId key = new (fileId, filePart);
        ObjectMetadata metadata = new (fileId, filePart, 
            (int)data.Length, DateTimeOffset.Now, true);
        return await SaveFile(data, key, metadata);
    }

    public ValueTask<Stream> GetFilePart(long fileId, int filePart)
    {
        return ValueTask.FromResult(GetFileStream(fileId, filePart));
    }

    public ValueTask<Stream> GetBigFilePart(long fileId, int filePart)
    {
        return ValueTask.FromResult(GetFileStream(fileId, filePart));
    }

    public IFileOwner GetFileOwner(UploadedFileInfoDTO fileInfo, int offset, 
        int limit, long reqMsgId, byte[] headerBytes)
    {
        return new LocalFileOwner(fileInfo, this, offset, limit, reqMsgId, headerBytes);
    }

    private Stream GetFileStream(long fileId, int filePart)
    {
        ObjectId key = new(fileId, filePart);
        _session.Read(key, out var metadata);
        var path = GetFilePath(metadata);
        return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }
}