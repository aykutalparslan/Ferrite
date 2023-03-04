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

using System.Security.Cryptography;
using System.Text;
using Autofac.Extras.Moq;
using Ferrite.Data.Repositories;
using Ferrite.TL.slim.layer150.dto;
using Xunit;

namespace Ferrite.Tests.Data;

public class FileInfoRepositoryTests
{
    [Fact]
    public void Puts_FileInfo()
    {
        using TLUploadedFileInfo info = GenerateFileInfo(false);
        var infoBytes = info.AsSpan().ToArray();
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(infoBytes, 
            (long)1357)).Verifiable();

        var repo = mock.Create<FileInfoRepository>();
        repo.PutFileInfo(info);
        store.VerifyAll();
    }
    
    [Fact]
    public void Puts_BigFileInfo()
    {
        using TLUploadedFileInfo info = GenerateFileInfo(true);
        var infoBytes = info.AsSpan().ToArray();
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(infoBytes, 
            (long)1357)).Verifiable();

        var repo = mock.Create<FileInfoRepository>();
        repo.PutBigFileInfo(info);
        store.VerifyAll();
    }
    
    [Fact]
    public void Gets_FileInfo()
    {
        using var info = GenerateFileInfo(false);
        var infoBytes = info.AsSpan().ToArray();
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Get((long)1357)).Returns(infoBytes);

        var repo = mock.Create<FileInfoRepository>();
        var result = repo.GetFileInfo(1357);
        Assert.NotNull(result);
        Assert.Equal(infoBytes, result.Value.AsSpan().ToArray());
    }
    
    [Fact]
    public void Gets_BigFileInfo()
    {
        using var info = GenerateFileInfo(true);
        var infoBytes = info.AsSpan().ToArray();
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Get((long)1357)).Returns(infoBytes);

        var repo = mock.Create<FileInfoRepository>();
        var result = repo.GetBigFileInfo(1357);
        Assert.NotNull(result);
        Assert.Equal(infoBytes, result.Value.AsSpan().ToArray());
    }
    
    [Fact]
    public void Puts_FilePart()
    {
        using TLFilePart part = GenerateFilePart();
        var partBytes = part.AsSpan().ToArray();
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(partBytes, 
            (long)123, 1)).Verifiable();

        var repo = mock.Create<FileInfoRepository>();
        repo.PutFilePart(part);
        store.VerifyAll();
    }
    
    [Fact]
    public void Puts_BigFilePart()
    {
        using TLFilePart part = GenerateFilePart();
        var partBytes = part.AsSpan().ToArray();
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(partBytes, 
            (long)123, 1)).Verifiable();

        var repo = mock.Create<FileInfoRepository>();
        repo.PutBigFilePart(part);
        store.VerifyAll();
    }
    
    [Fact]
    public void Gets_FileParts()
    {
        using TLFilePart part = GenerateFilePart();
        var partBytes = part.AsSpan().ToArray();
        List<byte[]> parts = new() { partBytes };
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Iterate((long)123)).Returns(parts);

        var repo = mock.Create<FileInfoRepository>();
        var result = repo.GetFileParts(123);
        Assert.Single(result);
        Assert.Equal(partBytes, result.ToArray()[0].AsSpan().ToArray());
    }
    
    [Fact]
    public void Gets_BigFileParts()
    {
        using TLFilePart part = GenerateFilePart();
        var partBytes = part.AsSpan().ToArray();
        List<byte[]> parts = new() { partBytes };
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Iterate((long)123)).Returns(parts);

        var repo = mock.Create<FileInfoRepository>();
        var result = repo.GetBigFileParts(123);
        Assert.Single(result);
        Assert.Equal(partBytes, result.ToArray()[0].AsSpan().ToArray());
    }
    
    [Fact]
    public void Puts_FileReference()
    {
        using TLFileReference reference = GenerateFileReference(false);
        var referenceBytes = reference.AsSpan().ToArray();
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        var refBytes = reference.AsFileReference().ReferenceBytes.ToArray();
        store.Setup(x => x.Put(referenceBytes, refBytes)).Verifiable();

        var repo = mock.Create<FileInfoRepository>();
        repo.PutFileReference(reference);
        store.VerifyAll();
    }
    
    [Fact]
    public void Gets_FileReference()
    {
        using TLFileReference reference = GenerateFileReference(false);
        var referenceBytes = reference.AsSpan().ToArray();
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        var refBytes = reference.AsFileReference().ReferenceBytes.ToArray();
        store.Setup(x => x.Get(refBytes)).Returns(referenceBytes);

        var repo = mock.Create<FileInfoRepository>();
        var result = repo.GetFileReference(refBytes);
        Assert.NotNull(result);
        Assert.Equal(referenceBytes, result.Value.AsSpan().ToArray());
    }

    private static TLFileReference GenerateFileReference(bool isBigFile)
    {
        return FileReference.Builder()
            .FileId(123)
            .IsBigFile(isBigFile)
            .ReferenceBytes(RandomNumberGenerator.GetBytes(16))
            .Build();
    }

    private static TLFilePart GenerateFilePart()
    {
        return FilePart.Builder()
            .PartSize(1024)
            .FileId(123)
            .PartNum(1)
            .Build();
    }

    private static TLUploadedFileInfo GenerateFileInfo(bool isBigFile)
    {
        return UploadedFileInfo.Builder()
            .Id(1357)
            .PartSize(1024)
            .Parts(3)
            .AccessHash(555)
            .Name("test"u8)
            .SavedOn(12345)
            .IsBigFile(isBigFile)
            .FileReference(RandomNumberGenerator.GetBytes(16))
            .Build();
    }
}