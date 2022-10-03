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

//TODO: complete implementation
public class MessageMediaMapper : ITLObjectMapper<MessageMedia, MessageMediaDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public MessageMediaMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public MessageMediaDTO MapToDTO(MessageMedia obj)
    {
        if (obj is MessageMediaEmptyImpl)
        {
            return new MessageMediaDTO(MessageMediaType.Empty, null, null,
                null, null, null, null, null, null,
                null, null, null, null, null, null,
                null, null, false, false, null, null,
                null, null, null, null, null, null,
                null, null, null, null,
                null);
        }
        if (obj is MessageMediaPhotoImpl photo)
        {
            var p = _mapper.MapToDTO<Photo, PhotoDTO>(photo.Photo);
            return new MessageMediaDTO(MessageMediaType.Photo, p, photo.TtlSeconds,
                null, null, null, null, null, null,
                null, null, null, null, null, null,
                null, null, false, false, null, null,
                null, null, null, null, null, null,
                null, null, null, null,
                null);
        }
        if (obj is MessageMediaGeoImpl geo)
        {
            var g = _mapper.MapToDTO<GeoPoint, GeoPointDTO>(geo.Geo);
            return new MessageMediaDTO(MessageMediaType.Geo, null, null,
                g, null, null, null, null, null,
                null, null, null, null, null, null,
                null, null, false, false, null, null,
                null, null, null, null, null, null,
                null, null, null, null,
                null);
        }
        if (obj is MessageMediaContactImpl contact)
        {
            return new MessageMediaDTO(MessageMediaType.Contact, null, null, null, 
                contact.PhoneNumber, contact.FirstName, contact.LastName, contact.Vcard, contact.UserId,
                null, null, null, null, null, null,
                null, null, false, false, null, null,
                null, null, null, null, null, null,
                null, null, null, null,
                null);
        }
        if (obj is MessageMediaUnsupportedImpl)
        {
            return new MessageMediaDTO(MessageMediaType.Unsupported, null, null,
                null, null, null, null, null, null,
                null, null, null, null, null, null,
                null, null, false, false, null, null,
                null, null, null, null, null, null,
                null, null, null, null,
                null);
        }
        throw new NotSupportedException();
    }

    public MessageMedia MapToTLObject(MessageMediaDTO obj)
    {
        if (obj.MessageMediaType == MessageMediaType.Empty)
        {
            var media = _factory.Resolve<MessageMediaEmptyImpl>();
            return media;
        }
        if (obj.MessageMediaType == MessageMediaType.Photo)
        {
            var media = _factory.Resolve<MessageMediaPhotoImpl>();
            if (obj.TtlSeconds != null)
            {
                media.TtlSeconds = (int)obj.TtlSeconds;
            }
            if (obj.Photo != null)
            {
                media.Photo = _mapper.MapToTLObject<Photo, PhotoDTO>(obj.Photo);
            }

            return media;
        }
        if (obj.MessageMediaType == MessageMediaType.Geo)
        {
            var media = _factory.Resolve<MessageMediaGeoImpl>();
            media.Geo = _mapper.MapToTLObject<GeoPoint, GeoPointDTO>(obj.Geo);
            return media;
        }
        if (obj.MessageMediaType == MessageMediaType.Contact)
        {
            var media = _factory.Resolve<MessageMediaContactImpl>();
            media.PhoneNumber = obj.PhoneNumber;
            media.FirstName = obj.FirstName;
            media.LastName = obj.LastName;
            media.Vcard = obj.VCard;
            media.UserId = (long)obj.UserId;
            return media;
        }
        if (obj.MessageMediaType == MessageMediaType.Unsupported)
        {
            return _factory.Resolve<MessageMediaUnsupportedImpl>();
        }

        throw new NotSupportedException();
    }
}