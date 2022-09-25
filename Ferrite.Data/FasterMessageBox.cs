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

public class FasterMessageBox : IMessageBox
{
    private readonly IAtomicCounter _ptsCounter;
    private readonly IAtomicCounter _messageIdCounter;
    private readonly IAtomicCounter _maxIdCounter;
    private readonly long _userId;
    private readonly FasterContext<string, SortedSet<long>> _unreadContext;
    private readonly FasterSortedSet<string> _dialogs;
    private readonly FasterContext<string, long> _counterContext;

    public FasterMessageBox(FasterContext<string, long> counterContext, 
        FasterContext<string, SortedSet<long>> unreadContext,
        FasterContext<string, SortedSet<string>> dialogContext,
        long userId)
    {
        _counterContext = counterContext;
        _unreadContext = unreadContext;
        _userId = userId;
        _dialogs = new FasterSortedSet<string>(dialogContext, $"msg:dialogs:{userId}");
        _ptsCounter = new FasterCounter(counterContext , $"seq:pts:{userId}");
        _messageIdCounter = new FasterCounter(counterContext , $"seq:message:id:{userId}");
    }
    public async ValueTask<int> Pts()
    {
        return (int)await _ptsCounter.Get();
    }

    public async ValueTask<int> IncrementPtsForMessage(PeerDTO peer, int messageId)
    {
        FasterSortedSet<long> unreadForPeer = new FasterSortedSet<long>(_unreadContext,
            $"msg:unread:{_userId}-{(int)peer.PeerType}-{peer.PeerId}");
        _dialogs.Add($"msg:unread:{_userId}-{(int)peer.PeerType}-{peer.PeerId}");
        unreadForPeer.Add(messageId);
        return (int)await _ptsCounter.IncrementAndGet();
    }

    public async ValueTask<int> NextMessageId()
    {
        int messageId = (int)await _messageIdCounter.IncrementAndGet();
        if (messageId == 0)
        {
            messageId = (int)await _messageIdCounter.IncrementAndGet();
        }
        return messageId;
    }

    public async ValueTask<int> ReadMessages(PeerDTO peer, int maxId)
    {
        FasterSortedSet<long> unreadForPeer = new FasterSortedSet<long>(_unreadContext,
            $"msg:unread:{_userId}-{(int)peer.PeerType}-{peer.PeerId}");
        unreadForPeer.RemoveEqualOrLess(maxId);
        if (unreadForPeer.Get().Count == 0)
        {
            _dialogs.Remove($"msg:unread:{_userId}-{(int)peer.PeerType}-{peer.PeerId}");
        }
        var peerMaxReadCounter = new FasterCounter(_counterContext , 
            $"msg:max-read:{_userId}-{(int)peer.PeerType}-{peer.PeerId}");
        return (int)await peerMaxReadCounter.IncrementTo(maxId);
    }

    public async ValueTask<int> ReadMessagesMaxId(PeerDTO peer)
    {
        var peerMaxReadCounter = new FasterCounter(_counterContext , 
            $"msg:max-read:{_userId}-{(int)peer.PeerType}-{peer.PeerId}");
        return (int)await peerMaxReadCounter.Get();
    }

    public ValueTask<int> UnreadMessages()
    {
        int unread = 0;
        var dialogs = _dialogs.Get();
        if(dialogs == null) return ValueTask.FromResult(unread);
        foreach (var d in dialogs)
        {
            FasterSortedSet<long> unreadForPeer = new FasterSortedSet<long>(_unreadContext, d);
            unread += unreadForPeer.Get().Count;
        }

        return ValueTask.FromResult(unread);
    }

    public ValueTask<int> UnreadMessages(PeerDTO peer)
    {
        FasterSortedSet<long> unreadForPeer = new FasterSortedSet<long>(_unreadContext, 
            $"msg:unread:{_userId}-{(int)peer.PeerType}-{peer.PeerId}");
        return ValueTask.FromResult(unreadForPeer.Get().Count);
    }

    public async ValueTask<int> IncrementPts()
    {
        int pts = (int)await _ptsCounter.IncrementAndGet();
        if (pts == 0)
        {
            pts = (int)await _ptsCounter.IncrementAndGet();
        }
        return pts;
    }
}