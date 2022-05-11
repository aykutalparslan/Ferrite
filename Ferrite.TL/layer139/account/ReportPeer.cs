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
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.layer139.account;
public class ReportPeer : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAccountService _account;
    private bool serialized = false;
    public ReportPeer(ITLObjectFactory objectFactory, IAccountService account)
    {
        factory = objectFactory;
        _account = account;
    }

    public int Constructor => -977650298;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_peer.TLBytes, false);
            writer.Write(_reason.TLBytes, false);
            writer.WriteTLString(_message);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private InputPeer _peer;
    public InputPeer Peer
    {
        get => _peer;
        set
        {
            serialized = false;
            _peer = value;
        }
    }

    private ReportReason _reason;
    public ReportReason Reason
    {
        get => _reason;
        set
        {
            serialized = false;
            _reason = value;
        }
    }

    private string _message;
    public string Message
    {
        get => _message;
        set
        {
            serialized = false;
            _message = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var inputPeer = _peer.Constructor switch
        {
            TLConstructor.InputPeerUser => new Data.InputPeer()
            {
                UserId = ((InputPeerUserImpl)_peer).UserId,
                AccessHash = ((InputPeerUserImpl)_peer).UserId,
            },
            TLConstructor.InputPeerChat => new Data.InputPeer()
            {
                ChatId = ((InputPeerChatImpl)_peer).ChatId,
            },
            TLConstructor.InputPeerUserFromMessage => new Data.InputPeer()
            {
                UserId = ((InputPeerUserFromMessageImpl)_peer).UserId,
                MsgId = ((InputPeerUserFromMessageImpl)_peer).MsgId,
                ChatId = ((InputPeerChatImpl)((InputPeerUserFromMessageImpl)_peer).Peer).ChatId
            },
            TLConstructor.InputPeerChannel => new Data.InputPeer()
            {
                ChannelId = ((InputPeerChannelImpl)_peer).ChannelId,
                AccessHash = ((InputPeerChannelImpl)_peer).AccessHash,
            },
            TLConstructor.InputPeerSelf => new Data.InputPeer(){ InputPeerType = InputPeerType.Self},
            TLConstructor.InputPeerChannelFromMessage => new Data.InputPeer()
            {
                ChannelId = ((InputPeerChannelFromMessageImpl)_peer).ChannelId,
                ChatId = ((InputPeerChatImpl)((InputPeerChannelFromMessageImpl)_peer).Peer).ChatId
            },
            _ => new Data.InputPeer(){ InputPeerType = InputPeerType.Empty},
        };
        var success = await _account.ReportPeer(ctx.AuthKeyId, inputPeer, _reason.Constructor switch
        {
            TLConstructor.InputReportReasonCopyright => Data.ReportReason.Copyright,
            TLConstructor.InputReportReasonFake => Data.ReportReason.Fake,
            TLConstructor.InputReportReasonPornography => Data.ReportReason.Pornography,
            TLConstructor.InputReportReasonSpam => Data.ReportReason.Spam,
            TLConstructor.InputReportReasonViolence => Data.ReportReason.Violence,
            TLConstructor.InputReportReasonChildAbuse => Data.ReportReason.ChildAbuse,
            TLConstructor.InputReportReasonGeoIrrelevant => Data.ReportReason.GeoIrrelevant,
            TLConstructor.InputReportReasonIllegalDrugs => Data.ReportReason.IllegalDrugs,
            TLConstructor.InputReportReasonPersonalDetails => Data.ReportReason.PersonalDetails,
            _ => Data.ReportReason.Other
        });
         
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        result.Result = success ? new BoolTrue() : new BoolFalse();
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _peer = (InputPeer)factory.Read(buff.ReadInt32(true), ref buff);
        _reason = (ReportReason)factory.Read(buff.ReadInt32(true), ref buff);
        _message = buff.ReadTLString();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}