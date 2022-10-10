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

public class InputMediaMapper : ITLObjectMapper<InputMedia, InputMediaDTO>
{
    private readonly IMapperContext _mapper;
    public InputMediaMapper(IMapperContext mapper)
    {
        _mapper = mapper;
    }
    public InputMediaDTO MapToDTO(InputMedia obj)
    {
        if (obj is InputMediaEmptyImpl)
        {
            return new InputMediaDTO
            {
                InputMediaType = InputMediaType.Empty,
            };
        }
        if (obj is InputMediaUploadedPhotoImpl uploadedPhoto)
        {
            List<InputDocumentDTO> stickers = null;
            if (uploadedPhoto.Stickers != null)
            {
                stickers = new List<InputDocumentDTO>();
                foreach (var s in uploadedPhoto.Stickers)
                {
                    stickers.Add(_mapper.MapToDTO<InputDocument, InputDocumentDTO>(s));
                }
            }
            return new InputMediaDTO
            {
                InputMediaType = InputMediaType.UploadedPhoto,
                File = _mapper.MapToDTO<InputFile, InputFileDTO>(uploadedPhoto.File),
                TtlSeconds = uploadedPhoto.TtlSeconds,
                Stickers = stickers,
            };
        }
        if (obj is InputMediaPhotoImpl photo)
        {
            return new InputMediaDTO
            {
                InputMediaType = InputMediaType.Photo,
                Photo = _mapper.MapToDTO<InputPhoto, InputPhotoDTO>(photo.Id),
                TtlSeconds = photo.TtlSeconds,
            };
        }
        if (obj is InputMediaGeoPointImpl geo)
        {
            return new InputMediaDTO
            {
                InputMediaType = InputMediaType.GeoPoint,
                GeoPoint = _mapper.MapToDTO<InputGeoPoint, InputGeoPointDTO>(geo.GeoPoint),
            };
        }
        if (obj is InputMediaContactImpl c)
        {
            return new InputMediaDTO
            {
                InputMediaType = InputMediaType.Contact,
                PhoneNumber = c.PhoneNumber,
                FirstName = c.FirstName,
                LastName = c.LastName,
                VCard = c.Vcard,
            };
        }
        if (obj is InputMediaUploadedDocumentImpl uploaded)
        {
            List<DocumentAttributeDTO> attributes = new();
            foreach (var a in uploaded.Attributes)
            {
                var attribute = _mapper.MapToDTO<DocumentAttribute, DocumentAttributeDTO>(a);
                attributes.Add(attribute);
            }
            List<InputDocumentDTO> stickers = new();
            if (uploaded.Flags[0])
            {
                foreach (var s in uploaded.Stickers)
                {
                    var sticker = _mapper.MapToDTO<InputDocument, InputDocumentDTO>(s);
                    stickers.Add(sticker);
                }
            }
            return new InputMediaDTO
            {
                InputMediaType = InputMediaType.UploadedDocument,
                NoSoundVideo = uploaded.NosoundVideo,
                ForceFile = uploaded.ForceFile,
                File = _mapper.MapToDTO<InputFile, InputFileDTO>(uploaded.File),
                Thumb = uploaded.Flags[2] ? _mapper.MapToDTO<InputFile, InputFileDTO>(uploaded.Thumb) : null,
                MimeType = uploaded.MimeType,
                Attributes = attributes,
                Stickers = uploaded.Flags[0] ? stickers : null,
                TtlSeconds = uploaded.Flags[1] ? uploaded.TtlSeconds : null,
            };
        }
        if (obj is InputMediaDocumentImpl document)
        {
            return new InputMediaDTO
            {
                InputMediaType = InputMediaType.Document,
                Document = _mapper.MapToDTO<InputDocument, InputDocumentDTO>(document.Id),
                TtlSeconds = document.Flags[0] ? document.TtlSeconds : null,
                Query = document.Flags[1] ? document.Query : null,
            };
        }
        if (obj is InputMediaGeoLiveImpl geoLive)
        {
            return new InputMediaDTO
            {
                InputMediaType = InputMediaType.GeoLive,
                Stopped = geoLive.Stopped,
                GeoPoint = _mapper.MapToDTO<InputGeoPoint, InputGeoPointDTO>(geoLive.GeoPoint),
                Heading = geoLive.Flags[2] ? geoLive.Heading : null,
                Period = geoLive.Flags[1] ? geoLive.Period : null,
                ProximityNotificationRadius = geoLive.Flags[3] ? geoLive.ProximityNotificationRadius : null,
            };
        }
        //TODO: implement mappings for the remaining media types
        throw new NotSupportedException();
    }

    public InputMedia MapToTLObject(InputMediaDTO obj)
    {
        throw new NotImplementedException();
    }
}