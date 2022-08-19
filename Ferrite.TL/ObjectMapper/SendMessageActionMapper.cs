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

public class SendMessageActionMapper : ITLObjectMapper<SendMessageAction, SendMessageActionDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public SendMessageActionMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }
    public SendMessageActionDTO MapToDTO(SendMessageAction obj)
    {
        if (obj is SendMessageTypingActionImpl)
        {
            return new SendMessageActionDTO(SendMessageActionType.TypingAction);
        }
        if (obj is SendMessageCancelActionImpl)
        {
            return new SendMessageActionDTO(SendMessageActionType.CancelAction);
        }
        if (obj is SendMessageRecordVideoActionImpl)
        {
            return new SendMessageActionDTO(SendMessageActionType.RecordVideoAction);
        }
        if (obj is SendMessageUploadVideoActionImpl video)
        {
            return new SendMessageActionDTO(SendMessageActionType.UploadVideoAction, video.Progress);
        }
        if (obj is SendMessageRecordAudioActionImpl)
        {
            return new SendMessageActionDTO(SendMessageActionType.RecordAudioAction);
        }
        if (obj is SendMessageUploadAudioActionImpl audio)
        {
            return new SendMessageActionDTO(SendMessageActionType.UploadAudioAction, audio.Progress);
        }
        if (obj is SendMessageUploadPhotoActionImpl photo)
        {
            return new SendMessageActionDTO(SendMessageActionType.UploadPhotoAction, photo.Progress);
        }
        if (obj is SendMessageUploadDocumentActionImpl document)
        {
            return new SendMessageActionDTO(SendMessageActionType.UploadDocumentAction, document.Progress);
        }
        if (obj is SendMessageGeoLocationActionImpl)
        {
            return new SendMessageActionDTO(SendMessageActionType.GeoLocationAction);
        }
        if (obj is SendMessageChooseContactActionImpl)
        {
            return new SendMessageActionDTO(SendMessageActionType.ChooseContactAction);
        }
        if (obj is SendMessageGamePlayActionImpl)
        {
            return new SendMessageActionDTO(SendMessageActionType.GamePlayAction);
        }
        if (obj is SendMessageRecordRoundActionImpl)
        {
            return new SendMessageActionDTO(SendMessageActionType.RecordRoundAction);
        }
        if (obj is SendMessageUploadRoundActionImpl round)
        {
            return new SendMessageActionDTO(SendMessageActionType.UploadRoundAction, round.Progress);
        }
        if (obj is SpeakingInGroupCallActionImpl)
        {
            return new SendMessageActionDTO(SendMessageActionType.SpeakingInGroupCallAction);
        }
        if (obj is SendMessageHistoryImportActionImpl history)
        {
            return new SendMessageActionDTO(SendMessageActionType.HistoryImportAction, history.Progress);
        }
        if (obj is SendMessageChooseStickerActionImpl)
        {
            return new SendMessageActionDTO(SendMessageActionType.ChooseStickerAction);
        }
        if (obj is SendMessageEmojiInteractionImpl emoji)
        {
            return new SendMessageActionDTO(SendMessageActionType.EmojiInteraction, Emoticon:emoji.Emoticon,
                MessageId:emoji.MsgId, Interaction:((DataJSONImpl)emoji.Interaction).Data);
        }
        if (obj is SendMessageEmojiInteractionSeenImpl seen)
        {
            return new SendMessageActionDTO(SendMessageActionType.EmojiInteraction, Emoticon:seen.Emoticon);
        }
        throw new NotSupportedException();
    }

    public SendMessageAction MapToTLObject(SendMessageActionDTO obj)
    {
        if (obj.SendMessageActionType == SendMessageActionType.TypingAction)
        {
            return _factory.Resolve<SendMessageTypingActionImpl>();
        }
        if (obj.SendMessageActionType == SendMessageActionType.CancelAction)
        {
            return _factory.Resolve<SendMessageCancelActionImpl>();
        }
        if (obj.SendMessageActionType == SendMessageActionType.RecordVideoAction)
        {
            return _factory.Resolve<SendMessageRecordVideoActionImpl>();
        }
        if (obj.SendMessageActionType == SendMessageActionType.UploadVideoAction)
        {
            var action = _factory.Resolve<SendMessageUploadVideoActionImpl>();
            action.Progress = (int)obj.Progress;
            return action;
        }
        if (obj.SendMessageActionType == SendMessageActionType.RecordAudioAction)
        {
            return _factory.Resolve<SendMessageRecordAudioActionImpl>();
        }
        if (obj.SendMessageActionType == SendMessageActionType.UploadAudioAction)
        {
            var action = _factory.Resolve<SendMessageUploadAudioActionImpl>();
            action.Progress = (int)obj.Progress;
            return action;
        }
        if (obj.SendMessageActionType == SendMessageActionType.UploadPhotoAction)
        {
            var action = _factory.Resolve<SendMessageUploadPhotoActionImpl>();
            action.Progress = (int)obj.Progress;
            return action;
        }
        if (obj.SendMessageActionType == SendMessageActionType.UploadDocumentAction)
        {
            var action = _factory.Resolve<SendMessageUploadDocumentActionImpl>();
            action.Progress = (int)obj.Progress;
            return action;
        }
        if (obj.SendMessageActionType == SendMessageActionType.GeoLocationAction)
        {
            return _factory.Resolve<SendMessageGeoLocationActionImpl>();
        }
        if (obj.SendMessageActionType == SendMessageActionType.ChooseContactAction)
        {
            return _factory.Resolve<SendMessageChooseContactActionImpl>();
        }
        if (obj.SendMessageActionType == SendMessageActionType.GamePlayAction)
        {
            return _factory.Resolve<SendMessageGamePlayActionImpl>();
        }
        if (obj.SendMessageActionType == SendMessageActionType.RecordRoundAction)
        {
            return _factory.Resolve<SendMessageRecordRoundActionImpl>();
        }
        if (obj.SendMessageActionType == SendMessageActionType.UploadRoundAction)
        {
            var action = _factory.Resolve<SendMessageUploadRoundActionImpl>();
            action.Progress = (int)obj.Progress;
            return action;
        }
        if (obj.SendMessageActionType == SendMessageActionType.SpeakingInGroupCallAction)
        {
            return _factory.Resolve<SpeakingInGroupCallActionImpl>();
        }
        if (obj.SendMessageActionType == SendMessageActionType.HistoryImportAction)
        {
            var action = _factory.Resolve<SendMessageHistoryImportActionImpl>();
            action.Progress = (int)obj.Progress;
            return action;
        }
        if (obj.SendMessageActionType == SendMessageActionType.ChooseStickerAction)
        {
            return _factory.Resolve<SendMessageChooseStickerActionImpl>();
        }
        if (obj.SendMessageActionType == SendMessageActionType.EmojiInteraction)
        {
            var action = _factory.Resolve<SendMessageEmojiInteractionImpl>();
            action.Emoticon = obj.Emoticon;
            var interaction = _factory.Resolve<DataJSONImpl>();
            interaction.Data = obj.Interaction;
            action.Interaction = interaction;
            action.MsgId = (int)obj.MessageId;
            return action;
        }
        if (obj.SendMessageActionType == SendMessageActionType.EmojiInteractionSeen)
        {
            var action = _factory.Resolve<SendMessageEmojiInteractionSeenImpl>();
            action.Emoticon = obj.Emoticon;
            return action;
        }
        throw new NotSupportedException();
    }
}