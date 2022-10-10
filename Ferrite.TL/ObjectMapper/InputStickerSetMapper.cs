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

public class InputStickerSetMapper : ITLObjectMapper<InputStickerSet, InputStickerSetDTO>
{
    private readonly ITLObjectFactory _factory;

    public InputStickerSetMapper(ITLObjectFactory factory)
    {
        _factory = factory;
    }
    public InputStickerSetDTO MapToDTO(InputStickerSet obj)
    {
        if (obj is InputStickerSetEmptyImpl)
        {
            return new InputStickerSetDTO(InputStickerSetType.Empty, null, null, 
                null, null);
        }
        if (obj is InputStickerSetIDImpl id)
        {
            return new InputStickerSetDTO(InputStickerSetType.Empty, id.Id, id.AccessHash, 
                null, null);
        }
        if (obj is InputStickerSetAnimatedEmojiImpl)
        {
            return new InputStickerSetDTO(InputStickerSetType.AnimatedEmoji, null, null, 
                null, null);
        }
        if (obj is InputStickerSetDiceImpl dice)
        {
            return new InputStickerSetDTO(InputStickerSetType.Dice, null, null, 
                null, dice.Emoticon);
        }
        if (obj is InputStickerSetAnimatedEmojiAnimationsImpl)
        {
            return new InputStickerSetDTO(InputStickerSetType.AnimatedEmojiAnimations, null, null, 
                null, null);
        }
        throw new NotSupportedException();
    }

    public InputStickerSet MapToTLObject(InputStickerSetDTO obj)
    {
        if (obj.Type == InputStickerSetType.Empty)
        {
            return _factory.Resolve<InputStickerSetEmptyImpl>();
        }
        if (obj.Type == InputStickerSetType.ID)
        {
            var set = _factory.Resolve<InputStickerSetIDImpl>();
            set.Id = (long)obj.ID!;
            set.AccessHash = (long)obj.AccessHash!;
            return set;
        }
        if (obj.Type == InputStickerSetType.AnimatedEmoji)
        {
            return _factory.Resolve<InputStickerSetAnimatedEmojiImpl>();
        }
        if (obj.Type == InputStickerSetType.ID)
        {
            var set = _factory.Resolve<InputStickerSetDiceImpl>();
            set.Emoticon = obj.Emoticon!;
            return set;
        }
        if (obj.Type == InputStickerSetType.AnimatedEmoji)
        {
            return _factory.Resolve<InputStickerSetAnimatedEmojiAnimationsImpl>();
        }
        throw new NotSupportedException();
    }
}