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
// but WITHOUT ANY WARRANTY) without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using System.Text;
using Ferrite.TL;
using Ferrite.TL.slim;
using Ferrite.TL.slim.layer150;
using Ferrite.TL.slim.mtproto;

namespace Ferrite.Core.Execution.Functions.BaseLayer.Help;

public class GetConfigFunc : ITLFunction
{
    private readonly IDataCenter _dataCenter;

    public GetConfigFunc(IDataCenter dataCenter)
    {
        _dataCenter = dataCenter;
    }
    public ValueTask<TLBytes?> Process(TLBytes q, TLExecutionContext ctx)
    {
        Vector dcOptions = new();
        for (int i = 1; i < 3; i++)
        {
            var ip = Encoding.UTF8.GetBytes(_dataCenter.IpAddress);
            var ip2 = Encoding.UTF8.GetString(ip);
            using var option = DcOption.Builder().Id(i)
                .IpAddress(ip)
                .Port(_dataCenter.Port)
                .MediaOnly(_dataCenter.MediaOnly)
                .Build();
            dcOptions.AppendTLObject(option.ToReadOnlySpan());
        }

        using var config = Config.Builder()
            .PhonecallsEnabled(true)
            .DefaultP2pContacts(true)
            .PreloadFeaturedStickers(false)
            .IgnorePhoneEntities(false)
            .RevokePmInbox(false)
            .BlockedMode(false)
            .Date((int)DateTimeOffset.Now.ToUnixTimeSeconds())
            .Expires((int)DateTimeOffset.Now.AddSeconds(90).ToUnixTimeSeconds())
            .TestMode(true)
            .ThisDc(1)
            .DcOptions(dcOptions)
            .DcTxtDomainName("localhost"u8)
            .ChatSizeMax(200)
            .MegagroupSizeMax(100000)
            .ForwardedCountMax(100)
            .OnlineUpdatePeriodMs(120000)
            .OfflineBlurTimeoutMs(5000)
            .OfflineIdleTimeoutMs(30000)
            .OnlineCloudTimeoutMs(300000)
            .NotifyCloudDelayMs(30000)
            .NotifyDefaultDelayMs(1500)
            .PushChatPeriodMs(60000)
            .PushChatLimit(2)
            .SavedGifsLimit(200)
            .EditTimeLimit(172800)
            .RevokeTimeLimit(172800)
            .RevokePmTimeLimit(172800)
            .RatingEDecay(2419200)
            .StickersRecentLimit(200)
            .StickersFavedLimit(5)
            .ChannelsReadMediaPeriod(604800)
            .PinnedDialogsCountMax(5)
            .CallReceiveTimeoutMs(20000)
            .CallRingTimeoutMs(90000)
            .CallConnectTimeoutMs(30000)
            .CallPacketTimeoutMs(10000)
            .MeUrlPrefix("localhost"u8)
            .GifSearchUsername("gif"u8)
            .VenueSearchUsername("foursquare"u8)
            .ImgSearchUsername("bing"u8)
            .CaptionLengthMax(1024)
            .MessageLengthMax(4096)
            .WebfileDcId(1).Build();
        var result = RpcResult.Builder()
            .ReqMsgId(ctx.MessageId)
            .Result(config.ToReadOnlySpan()).Build();
        return ValueTask.FromResult(result.TLBytes);
    }
}