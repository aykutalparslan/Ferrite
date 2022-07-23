// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation{ get; set; } public either version 3 of the License{ get; set; } public or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful{ get; set; } public
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not{ get; set; } public see <https://www.gnu.org/licenses/>.
// 


namespace Ferrite.Data;

public record DialogDTO()
{
    public DialogType DialogType { get; set; }
    public bool Pinned { get; set; }
    public bool UnreadMark { get; set; }
    public PeerDTO Peer { get; set; }
    public int TopMessage { get; set; }
    public int ReadInboxMaxId { get; set; }
    public int ReadOutboxMaxId { get; set; }
    public int UnreadCount { get; set; }
    public int UnreadMentionCount { get; set; }
    public int UnreadReactionsCount { get; set; }
    public PeerNotifySettingsDTO? NotifySettings { get; set; }
    public int Pts { get; set; }
    public DraftMessageDTO? DraftMessage { get; set; }
    public int? FolderId { get; set; }
    public FolderDTO? Folder { get; set; }
    public int UnreadMutedPeersCount { get; set; }
    public int UnreadUnmutedPeersCount { get; set; }
    public int UnreadMutedMessagesCount { get; set; }
    public int UnreadUnmutedMessagesCount { get; set; }
}
