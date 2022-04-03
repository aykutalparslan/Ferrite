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

namespace Ferrite.TL.layer139;
public abstract class Config : ITLObject
{
    public virtual int Constructor => throw new NotImplementedException() ; public virtual ReadOnlySequence<byte> TLBytes => throw new NotImplementedException() ; public virtual void Parse(ref SequenceReader buff)
    {
        throw new NotImplementedException();
    }

    public virtual void WriteTo(Span<byte> buff)
    {
        throw new NotImplementedException();
    }

    private static Config? _default;
    public static async Task<Config> GetDefaultConfigAsync(ITLObjectFactory factory)
    {
        if(_default != null)
        {
            return _default;
        }
        //TODO: Populate from the data store
        var config = factory.Resolve<ConfigImpl>();
        config.PhonecallsEnabled = true;
        config.DefaultP2pContacts = true;
        config.PreloadFeaturedStickers = false;
        config.IgnorePhoneEntities = false;
        config.RevokePmInbox = false;
        config.BlockedMode = false;
        config.Date = (int)DateTimeOffset.Now.ToUnixTimeSeconds();
        config.Expires = (int)DateTimeOffset.Now.AddDays(30).ToUnixTimeSeconds();
        config.TestMode = true;
        config.ThisDc = 1;
        config.DcOptions = await DcOption.GetDefaultDcOptionsAsync(factory);
        config.DcTxtDomainName = "localhost";
        config.ChatSizeMax = 200;
        config.MegagroupSizeMax = 100000;
        config.ForwardedCountMax = 100;
        config.OnlineUpdatePeriodMs = 120000;
        config.OfflineBlurTimeoutMs = 5000;
        config.OfflineIdleTimeoutMs = 30000;
        config.OnlineCloudTimeoutMs = 300000;
        config.NotifyCloudDelayMs = 30000;
        config.NotifyDefaultDelayMs = 1500;
        config.PushChatPeriodMs = 60000;
        config.PushChatLimit = 2;
        config.SavedGifsLimit = 200;
        config.EditTimeLimit = 172800;
        config.RevokeTimeLimit = 172800;
        config.RevokePmTimeLimit = 172800;
        config.RatingEDecay = 2419200;
        config.StickersRecentLimit = 200;
        config.StickersFavedLimit = 5;
        config.ChannelsReadMediaPeriod = 604800;
        config.PinnedDialogsCountMax = 5;
        config.CallReceiveTimeoutMs = 20000;
        config.CallRingTimeoutMs = 90000;
        config.CallConnectTimeoutMs = 30000;
        config.CallPacketTimeoutMs = 10000;
        config.MeUrlPrefix = "localhost";
        config.GifSearchUsername = "gif";
        config.VenueSearchUsername = "foursquare";
        config.ImgSearchUsername = "bing";
        config.CaptionLengthMax = 1024;
        config.MessageLengthMax = 4096;
        config.WebfileDcId = 1;
        if(_default == null)
        {
            _default = config;
        }
        return _default;
    }
}