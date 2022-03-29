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
public class BroadcastStatsImpl : BroadcastStats
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public BroadcastStatsImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -1107852396;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_period.TLBytes, false);
            writer.Write(_followers.TLBytes, false);
            writer.Write(_viewsPerPost.TLBytes, false);
            writer.Write(_sharesPerPost.TLBytes, false);
            writer.Write(_enabledNotifications.TLBytes, false);
            writer.Write(_growthGraph.TLBytes, false);
            writer.Write(_followersGraph.TLBytes, false);
            writer.Write(_muteGraph.TLBytes, false);
            writer.Write(_topHoursGraph.TLBytes, false);
            writer.Write(_interactionsGraph.TLBytes, false);
            writer.Write(_ivInteractionsGraph.TLBytes, false);
            writer.Write(_viewsBySourceGraph.TLBytes, false);
            writer.Write(_newFollowersBySourceGraph.TLBytes, false);
            writer.Write(_languagesGraph.TLBytes, false);
            writer.Write(_recentMessageInteractions.TLBytes, false);
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

    private StatsAbsValueAndPrev _followers;
    public StatsAbsValueAndPrev Followers
    {
        get => _followers;
        set
        {
            serialized = false;
            _followers = value;
        }
    }

    private StatsAbsValueAndPrev _viewsPerPost;
    public StatsAbsValueAndPrev ViewsPerPost
    {
        get => _viewsPerPost;
        set
        {
            serialized = false;
            _viewsPerPost = value;
        }
    }

    private StatsAbsValueAndPrev _sharesPerPost;
    public StatsAbsValueAndPrev SharesPerPost
    {
        get => _sharesPerPost;
        set
        {
            serialized = false;
            _sharesPerPost = value;
        }
    }

    private StatsPercentValue _enabledNotifications;
    public StatsPercentValue EnabledNotifications
    {
        get => _enabledNotifications;
        set
        {
            serialized = false;
            _enabledNotifications = value;
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

    private StatsGraph _followersGraph;
    public StatsGraph FollowersGraph
    {
        get => _followersGraph;
        set
        {
            serialized = false;
            _followersGraph = value;
        }
    }

    private StatsGraph _muteGraph;
    public StatsGraph MuteGraph
    {
        get => _muteGraph;
        set
        {
            serialized = false;
            _muteGraph = value;
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

    private StatsGraph _interactionsGraph;
    public StatsGraph InteractionsGraph
    {
        get => _interactionsGraph;
        set
        {
            serialized = false;
            _interactionsGraph = value;
        }
    }

    private StatsGraph _ivInteractionsGraph;
    public StatsGraph IvInteractionsGraph
    {
        get => _ivInteractionsGraph;
        set
        {
            serialized = false;
            _ivInteractionsGraph = value;
        }
    }

    private StatsGraph _viewsBySourceGraph;
    public StatsGraph ViewsBySourceGraph
    {
        get => _viewsBySourceGraph;
        set
        {
            serialized = false;
            _viewsBySourceGraph = value;
        }
    }

    private StatsGraph _newFollowersBySourceGraph;
    public StatsGraph NewFollowersBySourceGraph
    {
        get => _newFollowersBySourceGraph;
        set
        {
            serialized = false;
            _newFollowersBySourceGraph = value;
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

    private Vector<MessageInteractionCounters> _recentMessageInteractions;
    public Vector<MessageInteractionCounters> RecentMessageInteractions
    {
        get => _recentMessageInteractions;
        set
        {
            serialized = false;
            _recentMessageInteractions = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        buff.Skip(4); _period  =  factory . Read < StatsDateRangeDays > ( ref  buff ) ; 
        buff.Skip(4); _followers  =  factory . Read < StatsAbsValueAndPrev > ( ref  buff ) ; 
        buff.Skip(4); _viewsPerPost  =  factory . Read < StatsAbsValueAndPrev > ( ref  buff ) ; 
        buff.Skip(4); _sharesPerPost  =  factory . Read < StatsAbsValueAndPrev > ( ref  buff ) ; 
        buff.Skip(4); _enabledNotifications  =  factory . Read < StatsPercentValue > ( ref  buff ) ; 
        buff.Skip(4); _growthGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _followersGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _muteGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _topHoursGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _interactionsGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _ivInteractionsGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _viewsBySourceGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _newFollowersBySourceGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _languagesGraph  =  factory . Read < StatsGraph > ( ref  buff ) ; 
        buff.Skip(4); _recentMessageInteractions  =  factory . Read < Vector < MessageInteractionCounters > > ( ref  buff ) ; 
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}