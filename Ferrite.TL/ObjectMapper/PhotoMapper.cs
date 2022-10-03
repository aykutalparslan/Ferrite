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

using Ferrite.Data;
using Ferrite.TL.currentLayer;

namespace Ferrite.TL.ObjectMapper;

public class PhotoMapper : ITLObjectMapper<Photo, PhotoDTO>
{
    private readonly ITLObjectFactory _factory;
    public PhotoMapper(ITLObjectFactory factory)
    {
        _factory = factory;
    }
    public PhotoDTO MapToDTO(Photo obj)
    {
        if (obj is PhotoEmptyImpl empty)
        {
            return new PhotoDTO(true, false, empty.Id,
                null, null, null, null, 
                null,null);
        }
        else if(obj is PhotoImpl photo)
        {
            List<PhotoSizeDTO> photoSizes = new();
            if (photo.Sizes != null)
            {
                foreach (var s in photo.Sizes)
                {
                    if (s is PhotoSizeEmptyImpl es)
                    {
                        photoSizes.Add(new PhotoSizeDTO(PhotoSizeType.Empty,
                            es.Type, null, null, null, null, null));
                    }
                    else if (s is PhotoSizeImpl size)
                    {
                        photoSizes.Add(new PhotoSizeDTO(PhotoSizeType.Empty,
                            size.Type, size.W, size.H, size.Size, null, null));
                    }
                    else if (s is PhotoCachedSizeImpl cached)
                    {
                        photoSizes.Add(new PhotoSizeDTO(PhotoSizeType.Empty,
                            cached.Type, cached.W, cached.H, null, cached.Bytes, null));
                    }
                    else if (s is PhotoStrippedSizeImpl stripped)
                    {
                        photoSizes.Add(new PhotoSizeDTO(PhotoSizeType.Empty,
                            stripped.Type, null, null, null, stripped.Bytes, null));
                    }
                    else if (s is PhotoSizeProgressiveImpl progressive)
                    {
                        photoSizes.Add(new PhotoSizeDTO(PhotoSizeType.Empty,
                            progressive.Type, progressive.W, progressive.H, null, null, 
                            progressive.Sizes.ToList()));
                    }
                    else if (s is PhotoPathSizeImpl path)
                    {
                        photoSizes.Add(new PhotoSizeDTO(PhotoSizeType.Empty,
                            path.Type, null, null, null, path.Bytes, null));
                    }
                }
            }
            List<VideoSizeDTO> videoSizes = new();
            if (photo.VideoSizes != null)
            {
                foreach (var s in photo.VideoSizes)
                {
                    if (s is VideoSizeImpl size)
                    {
                        videoSizes.Add(new VideoSizeDTO(size.Type,
                            size.W, size.H, size.Size, size.VideoStartTs));
                    }
                }
            }
            return new PhotoDTO(false,
                photo.HasStickers,
                photo.Id,
                photo.AccessHash,
                photo.FileReference,
                photo.Date,
                photoSizes,
                videoSizes,
                photo.DcId);
        }

        throw new NotSupportedException();
    }

    public Photo MapToTLObject(PhotoDTO obj)
    {
        if (obj.Empty)
        {
            return _factory.Resolve<PhotoEmptyImpl>();
        }
        var photo = _factory.Resolve<PhotoImpl>();
        photo.Id = obj.Id;
        photo.AccessHash = (long)obj.AccessHash;
        photo.Date = (int)obj.Date;
        photo.DcId = (int)obj.DcId;
        photo.FileReference = obj.FileReference;
        photo.HasStickers = obj.HasStickers;
        photo.Sizes = _factory.Resolve<Vector<PhotoSize>>();
        foreach (var s in obj.Sizes)
        {
            var size = _factory.Resolve<PhotoSizeImpl>();
            size.Type = s.Type;
            size.Size = (int)s.Size;
            size.H = (int)s.H;
            size.W = (int)s.W;
            photo.Sizes.Add(size);
        }
        if (obj.VideoSizes is { Count: > 0 })
        {
            photo.VideoSizes = _factory.Resolve<Vector<VideoSize>>();
            foreach (var s in obj.VideoSizes)
            {
                var size = _factory.Resolve<VideoSizeImpl>();
                size.Type = s.Type;
                size.Size = s.Size;
                size.H = s.H;
                size.W = s.W;
                size.VideoStartTs = s.VideoStartTs;
                photo.VideoSizes.Add(size);
            }
        }

        return photo;
    }
}