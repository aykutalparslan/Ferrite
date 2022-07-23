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
    public Task<int> Pts();
    /// <summary>
    /// Increments the current event sequence number and
    /// adds an unread message to the message box for the peer.
    /// </summary>
    /// <param name="peer">The message destination.</param>
    /// <param name="messageId">the message Id.</param>
    /// <returns>Event sequence number after increment.</returns>
    public Task<int> IncrementPtsForMessage(PeerDTO peer, int messageId);
    /// <summary>
    /// Marks the messages with lower Id's than the <paramref name="maxId"/> as read.
    /// </summary>
    /// <param name="peer">The message destination.</param>
    /// <param name="maxId">The maximum Id for the messages to be read.</param>
    /// <returns>The number of unread messages remaining.</returns>
    public Task<int> ReadMessages(PeerDTO peer, int maxId);
    /// <summary>
    /// 
    /// </summary>
    /// <returns>Number of unread messages.</returns>
    public Task<int> UnreadMessages();
    /// <summary>
    ///  Increments the current event sequence number.
    /// </summary>
    /// <returns>Event sequence number after increment.</returns>
    public Task<int> IncrementPts();
}