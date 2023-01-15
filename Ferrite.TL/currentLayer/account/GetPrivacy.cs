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
using Ferrite.TL.ObjectMapper;

namespace Ferrite.TL.currentLayer.account;
public class GetPrivacy : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAccountService _accountService;
    private readonly IMapperContext _mapper;
    private bool serialized = false;
    public GetPrivacy(ITLObjectFactory objectFactory, IAccountService accountService, IMapperContext mapper)
    {
        factory = objectFactory;
        _accountService = accountService;
        _mapper = mapper;
    }

    public int Constructor => -623130288;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_key.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private InputPrivacyKey _key;
    public InputPrivacyKey Key
    {
        get => _key;
        set
        {
            serialized = false;
            _key = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        /*var authKeyId = ctx.PermAuthKeyId != 0 ? ctx.PermAuthKeyId : ctx.AuthKeyId;
        var privacyRules = await _accountService.GetPrivacy(authKeyId, GetPrivacyKey());
        var rulesResult = factory.Resolve<PrivacyRulesImpl>();
        var ruleList = factory.Resolve<Vector<PrivacyRule>>();
        foreach (var r in privacyRules.Rules)
        {
            ruleList.Add(_mapper.MapToTLObject<PrivacyRule, PrivacyRuleDTO>(r));
        }
        rulesResult.Rules = ruleList;
        var userList = factory.Resolve<Vector<User>>();
        foreach (var u in privacyRules.Users)
        {
            userList.Add(_mapper.MapToTLObject<User, UserDTO>(u));
        }
        rulesResult.Users = userList;
        var chatList = factory.Resolve<Vector<Chat>>();
        foreach (var c in privacyRules.Chats)
        {
            chatList.Add(_mapper.MapToTLObject<Chat, ChatDTO>(c));
        }
        rulesResult.Chats = chatList;
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        result.Result = rulesResult;
        return result;*/
        throw new NotImplementedException();
    }
    private Data.InputPrivacyKey GetPrivacyKey() => _key.Constructor switch
    {
        TLConstructor.InputPrivacyKeyStatusTimestamp => Data.InputPrivacyKey.StatusTimestamp,
        TLConstructor.InputPrivacyKeyChatInvite => Data.InputPrivacyKey.ChatInvite,
        TLConstructor.InputPrivacyKeyPhoneCall => Data.InputPrivacyKey.PhoneCall,
        TLConstructor.InputPrivacyKeyPhoneP2P => Data.InputPrivacyKey.PhoneP2P,
        TLConstructor.InputPrivacyKeyForwards => Data.InputPrivacyKey.Forwards,
        TLConstructor.InputPrivacyKeyProfilePhoto => Data.InputPrivacyKey.ProfilePhoto,
        TLConstructor.InputPrivacyKeyPhoneNumber => Data.InputPrivacyKey.PhoneNumber,
        _ => Data.InputPrivacyKey.AddedByPhone
    };

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _key = (InputPrivacyKey)factory.Read(buff.ReadInt32(true), ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}