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

namespace Ferrite.Data;

public interface IMessageBox
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns>Current event sequence number.</returns>
    public ValueTask<int> Pts();
    /// <summary>
    /// Increments the current event sequence number and
    /// adds an unread message to the message box for the peer.
    /// </summary>
    /// <param name="peer">The message source.</param>
    /// <param name="messageId">the message Id.</param>
    /// <returns>Event sequence number after increment.</returns>
    public ValueTask<int> IncrementPtsForMessage(PeerDTO peer, int messageId);
    /// <summary>
    /// Increments the MessageId counter.
    /// </summary>
    /// <returns>MessageId after the increment.</returns>
    public ValueTask<int> NextMessageId();
    /// <summary>
    /// Marks the messages with lower Id's than the <paramref name="maxId"/> as read.
    /// </summary>
    /// <param name="peer">The message source.</param>
    /// <param name="maxId">The maximum Id for the messages to be read.</param>
    /// <returns>The number of unread messages remaining.</returns>
    public ValueTask<int> ReadMessages(PeerDTO peer, int maxId);
    /// <summary>
    /// Retrieves the MaxId of the read messages for the <paramref name="peer"/>.
    /// </summary>
    /// <param name="peer">The message source.</param>
    /// <returns>MaxI.</returns>
    public ValueTask<int> ReadMessagesMaxId(PeerDTO peer);
    /// <summary>
    /// Retrieves the total number of unread messages.
    /// </summary>
    /// <returns>Total number of unread messages.</returns>
    public ValueTask<int> UnreadMessages();
    /// <summary>
    /// Retrieves the number of unread messages from the <paramref name="peer"/>.
    /// </summary>
    /// <param name="peer">The message source.</param>
    /// <returns>Number of unread messages.</returns>
    public ValueTask<int> UnreadMessages(PeerDTO peer);
    /// <summary>
    ///  Increments the current event sequence number.
    /// </summary>
    /// <returns>Event sequence number after increment.</returns>
    public ValueTask<int> IncrementPts();
}