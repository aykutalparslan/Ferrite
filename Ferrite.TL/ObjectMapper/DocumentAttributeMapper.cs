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

public class DocumentAttributeMapper : ITLObjectMapper<DocumentAttribute, DocumentAttributeDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public DocumentAttributeMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public DocumentAttributeDTO MapToDTO(DocumentAttribute obj)
    {
        if (obj is DocumentAttributeImageSizeImpl imageSize)
        {
            return new DocumentAttributeDTO(DocumentAttributeType.ImageSize,
                imageSize.W, imageSize.H, false, null, null,
                null, false, false,
                null, false, null, null, null, null);
        }
        if (obj is DocumentAttributeAnimatedImpl)
        {
            return new DocumentAttributeDTO(DocumentAttributeType.Animated,
                null, null, false, null, null,
                null, false, false,
                null, false, null, null, null, null);
        }
        if (obj is DocumentAttributeStickerImpl sticker)
        {
            return new DocumentAttributeDTO(DocumentAttributeType.Sticker,
                null, null, sticker.Flags[1], sticker.Alt, 
                _mapper.MapToDTO<InputStickerSet, InputStickerSetDTO>(sticker.Stickerset),
                sticker.Flags[0] ? _mapper.MapToDTO<MaskCoords, MaskCoordsDTO>(sticker.MaskCoords) : null, 
                false, false,
                null, false, null, null, null, null);
        }
        if (obj is DocumentAttributeVideoImpl video)
        {
            return new DocumentAttributeDTO(DocumentAttributeType.Video,
                video.W, video.H, false, null, null,
                null, video.RoundMessage, video.SupportsStreaming,
                video.Duration, false, null, null, null, null);
        }
        if (obj is DocumentAttributeAudioImpl audio)
        {
            return new DocumentAttributeDTO(DocumentAttributeType.Audio,
                null, null, false, null, null,
                null, false, false,
                audio.Duration, audio.Voice, audio.Flags[0] ? audio.Title : null,
                audio.Flags[1] ? audio.Performer : null,
                audio.Flags[0] ? audio.Waveform : null, null);
        }
        if (obj is DocumentAttributeFilenameImpl filename)
        {
            return new DocumentAttributeDTO(DocumentAttributeType.Filename,
                null, null, false, null, null,
                null, false, false,
                null, false, null, null, null, 
                filename.FileName);
        }
        if (obj is DocumentAttributeHasStickersImpl)
        {
            return new DocumentAttributeDTO(DocumentAttributeType.HasStickers,
                null, null, false, null, null,
                null, false, false,
                null, false, null, null, null, null);
        }
        throw new NotSupportedException();
    }

    public DocumentAttribute MapToTLObject(DocumentAttributeDTO obj)
    {
        throw new NotImplementedException();
    }
}