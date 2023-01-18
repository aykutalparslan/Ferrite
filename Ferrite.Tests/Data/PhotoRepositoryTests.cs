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
using Autofac.Extras.Moq;
using Ferrite.Data.Repositories;
using Ferrite.TL;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150;
using Moq;
using Xunit;

namespace Ferrite.Tests.Data;

public class PhotoRepositoryTests
{
    [Fact]
    public void Puts_ProfilePhoto()
    {
        var referenceBytes = RandomNumberGenerator.GetBytes(16);
        var date = DateTimeOffset.Now;
        var photoBytes = Photo.Builder()
            .Id(123)
            .AccessHash(1234)
            .FileReference(referenceBytes)
            .Date((int)date.ToUnixTimeSeconds())
            .DcId(2)
            .Sizes(new Vector()).Build().TLBytes!.Value.AsSpan().ToArray();
        
        
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Put(photoBytes, 
            (long)111, (long)123)).Verifiable();

        var repo = mock.Create<PhotoRepository>();

        repo.PutProfilePhoto(111, 123, 1234, referenceBytes, date);
        store.VerifyAll();
    }
    
    [Fact]
    public void Deletes_ProfilePhoto()
    {
        var mock = AutoMock.GetLoose();
        var store = mock.Mock<IKVStore>();
        store.Setup(x => x.Delete((long)111, (long)123)).Verifiable();
        var repo = mock.Create<PhotoRepository>();
        repo.DeleteProfilePhoto(111, 123);
        store.VerifyAll();
    }
    
    [Fact]
    public void PutsAndGets_ProfilePhotos()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new PhotoRepository(new RocksDBKVStore(ctx), new RocksDBKVStore(ctx));
        
        var referenceBytes1 = RandomNumberGenerator.GetBytes(16);
        var date1 = DateTimeOffset.Now;
        var photoBytes1 = Photo.Builder()
            .Id(123)
            .AccessHash(1234)
            .FileReference(referenceBytes1)
            .Date((int)date1.ToUnixTimeSeconds())
            .DcId(2)
            .Sizes(new Vector()).Build().TLBytes!.Value.AsSpan().ToArray();
        repo.PutProfilePhoto(111, 123, 1234, referenceBytes1, date1);
        
        var referenceBytes2 = RandomNumberGenerator.GetBytes(16);
        var date2 = DateTimeOffset.Now;
        var photoBytes2 = Photo.Builder()
            .Id(223)
            .AccessHash(2234)
            .FileReference(referenceBytes2)
            .Date((int)date2.ToUnixTimeSeconds())
            .DcId(2)
            .Sizes(new Vector()).Build().TLBytes!.Value.AsSpan().ToArray();
        repo.PutProfilePhoto(111, 223, 2234, referenceBytes2, date2);

        var photos = repo.GetProfilePhotos(111);
        Assert.Equal(2, photos.Count);
        Assert.Equal(photoBytes1, photos[0].AsSpan().ToArray());
        Assert.Equal(photoBytes2, photos[1].AsSpan().ToArray());
        Util.DeleteDirectory(path);
    }
    
    [Fact]
    public void PutsAndGets_ProfilePhoto()
    {
        string path = "test-" + Random.Shared.Next();
        var ctx = new RocksDBContext(path);
        var repo = new PhotoRepository(new RocksDBKVStore(ctx), new RocksDBKVStore(ctx));
        
        var referenceBytes1 = RandomNumberGenerator.GetBytes(16);
        var date1 = DateTimeOffset.Now;
        var photoBytes1 = Photo.Builder()
            .Id(123)
            .AccessHash(1234)
            .FileReference(referenceBytes1)
            .Date((int)date1.ToUnixTimeSeconds())
            .DcId(2)
            .Sizes(new Vector()).Build().TLBytes!.Value.AsSpan().ToArray();
        repo.PutProfilePhoto(111, 123, 1234, referenceBytes1, date1);

        var photo = repo.GetProfilePhoto(111, 123);
        Assert.NotNull(photo);
        Assert.Equal(photoBytes1, photo.Value.AsSpan().ToArray());
        Util.DeleteDirectory(path);
    }
}