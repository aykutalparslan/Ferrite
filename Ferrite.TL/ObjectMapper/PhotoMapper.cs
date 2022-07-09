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
        throw new NotImplementedException();
    }

    public Photo MapToTLObject(PhotoDTO obj)
    {
        var profilePhoto = _factory.Resolve<PhotoImpl>();
        profilePhoto.Id = obj.Id;
        profilePhoto.AccessHash = obj.AccessHash;
        profilePhoto.Date = obj.Date;
        profilePhoto.DcId = obj.DcId;
        profilePhoto.FileReference = obj.FileReference;
        profilePhoto.HasStickers = obj.HasStickers;
        profilePhoto.Sizes = _factory.Resolve<Vector<PhotoSize>>();
        foreach (var s in obj.Sizes)
        {
            var size = _factory.Resolve<PhotoSizeImpl>();
            size.Type = s.Type;
            size.Size = s.Size;
            size.H = s.H;
            size.W = s.W;
            profilePhoto.Sizes.Add(size);
        }
        if (obj.VideoSizes is { Count: > 0 })
        {
            profilePhoto.VideoSizes = _factory.Resolve<Vector<VideoSize>>();
            foreach (var s in obj.VideoSizes)
            {
                var size = _factory.Resolve<VideoSizeImpl>();
                size.Type = s.Type;
                size.Size = s.Size;
                size.H = s.H;
                size.W = s.W;
                size.VideoStartTs = s.VideoStartTs;
                profilePhoto.VideoSizes.Add(size);
            }
        }

        return profilePhoto;
    }
}