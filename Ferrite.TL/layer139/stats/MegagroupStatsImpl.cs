/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Buffers;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Utils;

namespace Ferrite.TL.layer139.stats;
public class MegagroupStatsImpl : MegagroupStats
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public MegagroupStatsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -276825834;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_period.TLBytes, false);
            writer.Write(_members.TLBytes, false);
            writer.Write(_messages.TLBytes, false);
            writer.Write(_viewers.TLBytes, false);
            writer.Write(_posters.TLBytes, false);
            writer.Write(_growthGraph.TLBytes, false);
            writer.Write(_membersGraph.TLBytes, false);
            writer.Write(_newMembersBySourceGraph.TLBytes, false);
            writer.Write(_languagesGraph.TLBytes, false);
            writer.Write(_messagesGraph.TLBytes, false);
            writer.Write(_actionsGraph.TLBytes, false);
            writer.Write(_topHoursGraph.TLBytes, false);
            writer.Write(_weekdaysGraph.TLBytes, false);
            writer.Write(_topPosters.TLBytes, false);
            writer.Write(_topAdmins.TLBytes, false);
            writer.Write(_topInviters.TLBytes, false);
            writer.Write(_users.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private StatsDateRangeDays _period;
    public StatsDateRangeDays Period
    {
        get => _period;
        set
        {
            serialized = false;
            _period = value;
        }
    }

    private StatsAbsValueAndPrev _members;
    public StatsAbsValueAndPrev Members
    {
        get => _members;
        set
        {
            serialized = false;
            _members = value;
        }
    }

    private StatsAbsValueAndPrev _messages;
    public StatsAbsValueAndPrev Messages
    {
        get => _messages;
        set
        {
            serialized = false;
            _messages = value;
        }
    }

    private StatsAbsValueAndPrev _viewers;
    public StatsAbsValueAndPrev Viewers
    {
        get => _viewers;
        set
        {
            serialized = false;
            _viewers = value;
        }
    }

    private StatsAbsValueAndPrev _posters;
    public StatsAbsValueAndPrev Posters
    {
        get => _posters;
        set
        {
            serialized = false;
            _posters = value;
        }
    }

    private StatsGraph _growthGraph;
    public StatsGraph GrowthGraph
    {
        get => _growthGraph;
        set
        {
            serialized = false;
            _growthGraph = value;
        }
    }

    private StatsGraph _membersGraph;
    public StatsGraph MembersGraph
    {
        get => _membersGraph;
        set
        {
            serialized = false;
            _membersGraph = value;
        }
    }

    private StatsGraph _newMembersBySourceGraph;
    public StatsGraph NewMembersBySourceGraph
    {
        get => _newMembersBySourceGraph;
        set
        {
            serialized = false;
            _newMembersBySourceGraph = value;
        }
    }

    private StatsGraph _languagesGraph;
    public StatsGraph LanguagesGraph
    {
        get => _languagesGraph;
        set
        {
            serialized = false;
            _languagesGraph = value;
        }
    }

    private StatsGraph _messagesGraph;
    public StatsGraph MessagesGraph
    {
        get => _messagesGraph;
        set
        {
            serialized = false;
            _messagesGraph = value;
        }
    }

    private StatsGraph _actionsGraph;
    public StatsGraph ActionsGraph
    {
        get => _actionsGraph;
        set
        {
            serialized = false;
            _actionsGraph = value;
        }
    }

    private StatsGraph _topHoursGraph;
    public StatsGraph TopHoursGraph
    {
        get => _topHoursGraph;
        set
        {
            serialized = false;
            _topHoursGraph = value;
        }
    }

    private StatsGraph _weekdaysGraph;
    public StatsGraph WeekdaysGraph
    {
        get => _weekdaysGraph;
        set
        {
            serialized = false;
            _weekdaysGraph = value;
        }
    }

    private Vector<StatsGroupTopPoster> _topPosters;
    public Vector<StatsGroupTopPoster> TopPosters
    {
        get => _topPosters;
        set
        {
            serialized = false;
            _topPosters = value;
        }
    }

    private Vector<StatsGroupTopAdmin> _topAdmins;
    public Vector<StatsGroupTopAdmin> TopAdmins
    {
        get => _topAdmins;
        set
        {
            serialized = false;
            _topAdmins = value;
        }
    }

    private Vector<StatsGroupTopInviter> _topInviters;
    public Vector<StatsGroupTopInviter> TopInviters
    {
        get => _topInviters;
        set
        {
            serialized = false;
            _topInviters = value;
        }
    }

    private Vector<User> _users;
    public Vector<User> Users
    {
        get => _users;
        set
        {
            serialized = false;
            _users = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        buff.Skip(4); _period  =  factory . Read < StatsDateRangeDays > ( ref  buff ) ; 
        buff.Skip(4); _members  =  factory . Read < StatsAbsValueAndPrev > ( ref  buff ) ; 
        buff.Skip(4); _messages  =  factory . Read < StatsAbsValueAndPrev > ( ref  buff ) ; 
        buff.Skip(4); _viewers  =  factory . Read < StatsAbsValueAndPrev > ( ref  buff ) ; 
        buff.Skip(4); _posters  =  factory . Read < StatsAbsValueAndPrev > ( ref  buff ) ; 
        buff.Skip(4); _growthGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _membersGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _newMembersBySourceGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _languagesGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _messagesGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _actionsGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _topHoursGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _weekdaysGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _topPosters  =  factory . Read < Vector < StatsGroupTopPoster > > ( ref  buff ) ; 
        buff.Skip(4); _topAdmins  =  factory . Read < Vector < StatsGroupTopAdmin > > ( ref  buff ) ; 
        buff.Skip(4); _topInviters  =  factory . Read < Vector < StatsGroupTopInviter > > ( ref  buff ) ; 
        buff.Skip(4); _users  =  factory . Read < Vector < User > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}