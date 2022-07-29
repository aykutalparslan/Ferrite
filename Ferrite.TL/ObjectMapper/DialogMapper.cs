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

public class DialogMapper : ITLObjectMapper<Dialog, DialogDTO>
{
    private readonly ITLObjectFactory _factory;
    private readonly IMapperContext _mapper;
    public DialogMapper(ITLObjectFactory factory, IMapperContext mapper)
    {
        _factory = factory;
        _mapper = mapper;
    }

    public DialogDTO MapToDTO(Dialog obj)
    {
        throw new NotImplementedException();
    }

    public Dialog MapToTLObject(DialogDTO obj)
    {
        if (obj.DialogType == DialogType.Dialog)
        {
            var dialog = _factory.Resolve<DialogImpl>();
            dialog.Pinned = obj.Pinned;
            dialog.UnreadMark = obj.UnreadMark;
            dialog.Peer = _mapper.MapToTLObject<Peer, PeerDTO>(obj.Peer);
            dialog.TopMessage = obj.TopMessage;
            dialog.ReadInboxMaxId = obj.ReadInboxMaxId;
            dialog.ReadOutboxMaxId = obj.ReadOutboxMaxId;
            dialog.UnreadCount = obj.UnreadCount;
            dialog.UnreadMentionsCount = obj.UnreadMentionsCount;
            dialog.UnreadReactionsCount = obj.UnreadReactionsCount;
            dialog.NotifySettings =
                _mapper.MapToTLObject<PeerNotifySettings, PeerNotifySettingsDTO>(obj.NotifySettings);
            if (obj.Pts != null)
            {
                dialog.Pts = (int)obj.Pts;
            }

            if (obj.DraftMessage != null)
            {
                if (obj.DraftMessage.DraftMessageType == DraftMessageType.Empty)
                {
                    dialog.Draft = _factory.Resolve<DraftMessageEmptyImpl>();
                }
                else if (obj.DraftMessage.DraftMessageType == DraftMessageType.Draft)
                {
                    var draft = _factory.Resolve<DraftMessageImpl>();
                    draft.NoWebpage = obj.DraftMessage.NoWebPage;
                    if (obj.DraftMessage.ReplyToMessageId != null)
                    {
                        draft.ReplyToMsgId = (int)obj.DraftMessage.ReplyToMessageId;
                    }
                    draft.Message = obj.DraftMessage.Message;
                    if (obj.DraftMessage.Entities != null)
                    {
                        draft.Entities = _factory.Resolve<Vector<MessageEntity>>();
                        foreach (var e in obj.DraftMessage.Entities)
                        {
                            draft.Entities.Add(_mapper.MapToTLObject<MessageEntity, MessageEntityDTO>(e));
                        }
                    }
                    draft.Date = (int)obj.DraftMessage.Date;
                    dialog.Draft = draft;
                }
            }
            if (obj.FolderId != null)
            {
                dialog.FolderId = (int)obj.FolderId;
            }

            return dialog;
        }
        else if (obj.DialogType == DialogType.Dialog)
        {
            var dialog = _factory.Resolve<DialogFolderImpl>();
            dialog.Pinned = obj.Pinned;
            var folder = _factory.Resolve<FolderImpl>();
            folder.AutofillNewBroadcasts = obj.Folder.AutofillNewBroadcasts;
            folder.AutofillPublicGroups = obj.Folder.AutofillPublicGroups;
            folder.AutofillNewCorrespondents = obj.Folder.AutofillNewCorrespondents;
            folder.Id = obj.Folder.Id;
            folder.Title = obj.Folder.Title;
            if (obj.Folder.Photo != null)
            {
                if (obj.Folder.Photo.Empty)
                {
                    folder.Photo = _factory.Resolve<ChatPhotoEmptyImpl>();
                }
                else
                {
                    var photo = _factory.Resolve<ChatPhotoImpl>();
                    photo.HasVideo = obj.Folder.Photo.HasVideo;
                    photo.PhotoId = obj.Folder.Photo.PhotoId;
                    if (obj.Folder.Photo.StrippedThumb != null)
                    {
                        photo.StrippedThumb = obj.Folder.Photo.StrippedThumb;
                    }

                    photo.DcId = obj.Folder.Photo.DcId;
                    folder.Photo = photo;
                }
            }
            dialog.Folder = folder;
            dialog.Peer = _mapper.MapToTLObject<Peer, PeerDTO>(obj.Peer);
            dialog.TopMessage = obj.TopMessage;
            dialog.UnreadMutedPeersCount = obj.UnreadMutedPeersCount;
            dialog.UnreadUnmutedPeersCount = obj.UnreadUnmutedPeersCount;
            dialog.UnreadMutedMessagesCount = obj.UnreadMutedMessagesCount;
            dialog.UnreadUnmutedMessagesCount = obj.UnreadUnmutedMessagesCount;
            return dialog;
        }
        throw new NotSupportedException();
    }
}