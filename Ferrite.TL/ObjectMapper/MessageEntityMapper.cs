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

public class MessageEntityMapper : ITLObjectMapper<MessageEntity, MessageEntityDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public MessageEntityMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public MessageEntityDTO MapToDTO(MessageEntity obj)
    {
        if (obj is MessageEntityUnknownImpl unknownImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.Unknown,
                Offset = unknownImpl.Offset,
                Length = unknownImpl.Length,
            };
        }
        if (obj is MessageEntityMentionImpl mentionImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.Mention,
                Offset = mentionImpl.Offset,
                Length = mentionImpl.Length,
            };
        }
        if (obj is MessageEntityHashtagImpl hashtagImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.Hashtag,
                Offset = hashtagImpl.Offset,
                Length = hashtagImpl.Length,
            };
        }
        if (obj is MessageEntityBotCommandImpl botCommandImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.BotCommand,
                Offset = botCommandImpl.Offset,
                Length = botCommandImpl.Length,
            };
        }
        if (obj is MessageEntityUrlImpl urlImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.Url,
                Offset = urlImpl.Offset,
                Length = urlImpl.Length,
            };
        }
        if (obj is MessageEntityEmailImpl emailImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.Email,
                Offset = emailImpl.Offset,
                Length = emailImpl.Length,
            };
        }
        if (obj is MessageEntityBoldImpl boldImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.Bold,
                Offset = boldImpl.Offset,
                Length = boldImpl.Length,
            };
        }
        if (obj is MessageEntityItalicImpl italicImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.Italic,
                Offset = italicImpl.Offset,
                Length = italicImpl.Length,
            };
        }
        if (obj is MessageEntityCodeImpl codeImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.Code,
                Offset = codeImpl.Offset,
                Length = codeImpl.Length,
            };
        }
        if (obj is MessageEntityPreImpl preImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.Pre,
                Offset = preImpl.Offset,
                Length = preImpl.Length,
                Language = preImpl.Language
            };
        }
        if (obj is MessageEntityTextUrlImpl textUrlImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.TextUrl,
                Offset = textUrlImpl.Offset,
                Length = textUrlImpl.Length,
                Url = textUrlImpl.Url
            };
        }
        if (obj is MessageEntityMentionNameImpl mentionNameImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.MentionName,
                Offset = mentionNameImpl.Offset,
                Length = mentionNameImpl.Length,
                UserId = mentionNameImpl.UserId
            };
        }
        if (obj is InputMessageEntityMentionNameImpl inputMentionNameImpl)
        {
            var userId = _mapper.MapToDTO<InputUser, InputUserDTO>(inputMentionNameImpl.UserId);
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.MentionName,
                Offset = inputMentionNameImpl.Offset,
                Length = inputMentionNameImpl.Length,
                User = userId
            };
        }
        if (obj is MessageEntityPhoneImpl phoneImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.Phone,
                Offset = phoneImpl.Offset,
                Length = phoneImpl.Length,
            };
        }
        if (obj is MessageEntityCashtagImpl cashtagImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.Cashtag,
                Offset = cashtagImpl.Offset,
                Length = cashtagImpl.Length,
            };
        }
        if (obj is MessageEntityUnderlineImpl underlineImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.Underline,
                Offset = underlineImpl.Offset,
                Length = underlineImpl.Length,
            };
        }
        if (obj is MessageEntityStrikeImpl strikeImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.Strike,
                Offset = strikeImpl.Offset,
                Length = strikeImpl.Length,
            };
        }
        if (obj is MessageEntityBlockquoteImpl blockquoteImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.Blockquote,
                Offset = blockquoteImpl.Offset,
                Length = blockquoteImpl.Length,
            };
        }
        if (obj is MessageEntityBankCardImpl bankCardImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.BankCard,
                Offset = bankCardImpl.Offset,
                Length = bankCardImpl.Length,
            };
        }
        if (obj is MessageEntitySpoilerImpl boldItalicImpl)
        {
            return new MessageEntityDTO
            {
                MessageEntityType = MessageEntityType.Spoiler,
                Offset = boldItalicImpl.Offset,
                Length = boldItalicImpl.Length,
            };
        }

        throw new NotSupportedException("Unknown MessageEntity type");
    }

    public MessageEntity MapToTLObject(MessageEntityDTO obj)
    {
        if(obj.MessageEntityType == MessageEntityType.Unknown)
        {
            var entity =  _factory.Resolve<MessageEntityUnknownImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.Mention)
        {
            var entity = _factory.Resolve<MessageEntityMentionImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.Hashtag)
        {
            var entity = _factory.Resolve<MessageEntityHashtagImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.BotCommand)
        {
            var entity = _factory.Resolve<MessageEntityBotCommandImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.Url)
        {
            var entity = _factory.Resolve<MessageEntityUrlImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.Email)
        {
            var entity = _factory.Resolve<MessageEntityEmailImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.Bold)
        {
            var entity = _factory.Resolve<MessageEntityBoldImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.Italic)
        {
            var entity = _factory.Resolve<MessageEntityItalicImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.Code)
        {
            var entity = _factory.Resolve<MessageEntityCodeImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.Pre)
        {
            var entity = _factory.Resolve<MessageEntityPreImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            entity.Language = obj.Language;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.TextUrl)
        {
            var entity = _factory.Resolve<MessageEntityTextUrlImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            entity.Url = obj.Url;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.MentionName)
        {
            var entity = _factory.Resolve<MessageEntityMentionNameImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            entity.UserId = obj.UserId;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.InputMentionName)
        {
            var entity = _factory.Resolve<InputMessageEntityMentionNameImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            entity.UserId = _mapper.MapToTLObject<InputUser, InputUserDTO>(obj.User);
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.Phone)
        {
            var entity = _factory.Resolve<MessageEntityPhoneImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.Cashtag)
        {
            var entity = _factory.Resolve<MessageEntityCashtagImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.Underline)
        {
            var entity = _factory.Resolve<MessageEntityUnderlineImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.Strike)
        {
            var entity = _factory.Resolve<MessageEntityStrikeImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.Blockquote)
        {
            var entity = _factory.Resolve<MessageEntityBlockquoteImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.BankCard)
        {
            var entity = _factory.Resolve<MessageEntityBankCardImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        if(obj.MessageEntityType == MessageEntityType.Spoiler)
        {
            var entity = _factory.Resolve<MessageEntitySpoilerImpl>();
            entity.Length = obj.Length;
            entity.Offset = obj.Offset;
            return entity;
        }
        throw new NotSupportedException();
    }
}