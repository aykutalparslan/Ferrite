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
using Ferrite.TL.layer139.messages;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.layer139.account;
public class SetPrivacy : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly IAccountService _accountService;
    private bool serialized = false;
    public SetPrivacy(ITLObjectFactory objectFactory, IAccountService accountService)
    {
        factory = objectFactory;
        _accountService = accountService;
    }

    public int Constructor => -906486552;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write(_key.TLBytes, false);
            writer.Write(_rules.TLBytes, false);
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

    private Vector<InputPrivacyRule> _rules;
    public Vector<InputPrivacyRule> Rules
    {
        get => _rules;
        set
        {
            serialized = false;
            _rules = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var authKeyId = ctx.PermAuthKeyId != 0 ? ctx.PermAuthKeyId : ctx.AuthKeyId;
        var privacyRules = await _accountService.SetPrivacy(authKeyId, GetPrivacyKey(), GetPrivacyRules());
        var rulesResult = factory.Resolve<PrivacyRulesImpl>();
        var ruleList = factory.Resolve<Vector<PrivacyRule>>();
        foreach (var r in privacyRules.Rules)
        {
            ruleList.Add(CreatePrivacyRule(r));
        }
        rulesResult.Rules = ruleList;
        var userList = factory.Resolve<Vector<User>>();
        foreach (var u in privacyRules.Users)
        {
            userList.Add(CreateUser(u));
        }
        rulesResult.Users = userList;
        var chatList = factory.Resolve<Vector<Chat>>();
        foreach (var c in privacyRules.Chats)
        {
            
            chatList.Add(CreateChat(c));
        }
        rulesResult.Chats = chatList;
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        result.Result = rulesResult;
        return result;
    }

    private Chat CreateChat(Data.Chat c)
    {
        if (c.ChatType == ChatType.Chat)
        {
            var chat = factory.Resolve<ChatImpl>();
            chat.Id = c.Id;
            chat.Title = c.Title;
            chat.Photo = factory.Resolve<ChatPhotoEmptyImpl>();
            chat.ParticipantsCount = c.ParticipantsCount;
            chat.Date = c.Date;
            chat.Version = c.Version;
            return chat;
        }
        else if (c.ChatType == ChatType.Channel)
        {
            var chat = factory.Resolve<ChannelImpl>();
            chat.Id = c.Id;
            chat.Title = c.Title;
            chat.Photo = factory.Resolve<ChatPhotoEmptyImpl>();
            chat.ParticipantsCount = c.ParticipantsCount;
            chat.Date = c.Date;
            return chat;
        }
        else if (c.ChatType == ChatType.ChatForbidden)
        {
            var chat = factory.Resolve<ChatForbiddenImpl>();
            chat.Id = c.Id;
            chat.Title = c.Title;
            return chat;
        }
        else if (c.ChatType == ChatType.ChannelForbidden)
        {
            var chat = factory.Resolve<ChannelForbiddenImpl>();
            chat.Id = c.Id;
            chat.Title = c.Title;
            chat.AccessHash = c.AccessHash;
            return chat;
        }
        return factory.Resolve<ChatEmptyImpl>();
    }

    private User CreateUser(Data.User u)
    {
        if (u.Empty)
        {
            return factory.Resolve<UserEmptyImpl>();
        }
        else
        {
            var user = factory.Resolve<UserImpl>();
            user.Id = u.Id;
            user.FirstName = u.FirstName;
            user.LastName = u.LastName;
            user.Phone = u.Phone;
            user.Self = u.Self;
            if (u.Status == Data.UserStatus.Empty)
            {
                user.Status = factory.Resolve<UserStatusEmptyImpl>();
            }

            if (u.Photo.Empty)
            {
                user.Photo = factory.Resolve<UserProfilePhotoEmptyImpl>();
            }

            return user;
        }
    }

    private PrivacyRule CreatePrivacyRule(Data.PrivacyRule value)
    {
        if (value.PrivacyRuleType == PrivacyRuleType.AllowAll)
        {
            return factory.Resolve<PrivacyValueAllowAllImpl>();
        }
        else if (value.PrivacyRuleType == PrivacyRuleType.AllowUsers)
        {
            var rule = factory.Resolve<PrivacyValueAllowUsersImpl>();
            rule.Users = factory.Resolve<VectorOfLong>();
            foreach (var p in value.Peers)
            {
                rule.Users.Add(p);
            }

            return rule;
        }
        else if (value.PrivacyRuleType == PrivacyRuleType.DisallowContacts)
        {
            return factory.Resolve<PrivacyValueDisallowContactsImpl>();
        }
        else if (value.PrivacyRuleType == PrivacyRuleType.DisallowAll)
        {
            return factory.Resolve<PrivacyValueDisallowAllImpl>();
        }
        else if (value.PrivacyRuleType == PrivacyRuleType.DisallowUsers)
        {
            var rule = factory.Resolve<PrivacyValueDisallowUsersImpl>();
            rule.Users = factory.Resolve<VectorOfLong>();
            foreach (var p in value.Peers)
            {
                rule.Users.Add(p);
            }

            return rule;
        }
        else if (value.PrivacyRuleType == PrivacyRuleType.AllowChatParticipants)
        {
            var rule = factory.Resolve<PrivacyValueAllowChatParticipantsImpl>();
            rule.Chats = factory.Resolve<VectorOfLong>();
            foreach (var p in value.Peers)
            {
                rule.Chats.Add(p);
            }

            return rule;
        }
        else if (value.PrivacyRuleType == PrivacyRuleType.DisallowChatParticipants)
        {
            var rule = factory.Resolve<PrivacyValueDisallowChatParticipantsImpl>();
            rule.Chats = factory.Resolve<VectorOfLong>();
            foreach (var p in value.Peers)
            {
                rule.Chats.Add(p);
            }

            return rule;
        }
        return factory.Resolve<PrivacyValueAllowContactsImpl>();
    }

    private List<Data.PrivacyRule> GetPrivacyRules()
    {
        List<Data.PrivacyRule> rules = new();
        foreach (var r in _rules)
        {
            if (r.Constructor == TLConstructor.InputPrivacyValueAllowContacts)
            {
                rules.Add(new Data.PrivacyRule()
                    { PrivacyRuleType = PrivacyRuleType.AllowContacts });
            }
            else if (r.Constructor == TLConstructor.InputPrivacyValueAllowAll)
            {
                rules.Add(new Data.PrivacyRule()
                    { PrivacyRuleType = PrivacyRuleType.AllowAll });
            }
            else if (r.Constructor == TLConstructor.InputPrivacyValueAllowUsers)
            {
                List<long> userIds = new();
                foreach (var user in ((InputPrivacyValueAllowUsersImpl)r).Users)
                {
                    userIds.Add(user.GetUserId());
                }

                rules.Add(new Data.PrivacyRule()
                {
                    PrivacyRuleType = PrivacyRuleType.AllowUsers,
                    Peers = userIds
                });
            }
            else if (r.Constructor == TLConstructor.InputPrivacyValueDisallowContacts)
            {
                rules.Add(new Data.PrivacyRule()
                    { PrivacyRuleType = PrivacyRuleType.DisallowContacts });
            }
            else if (r.Constructor == TLConstructor.InputPrivacyValueDisallowAll)
            {
                rules.Add(new Data.PrivacyRule()
                    { PrivacyRuleType = PrivacyRuleType.DisallowAll });
            }
            else if (r.Constructor == TLConstructor.InputPrivacyValueDisallowUsers)
            {
                List<long> userIds = new();
                foreach (var user in ((InputPrivacyValueAllowUsersImpl)r).Users)
                {
                    userIds.Add(user.GetUserId());
                }

                rules.Add(new Data.PrivacyRule()
                {
                    PrivacyRuleType = PrivacyRuleType.DisallowUsers,
                    Peers = userIds
                });
            }
            else if (r.Constructor == TLConstructor.InputPrivacyValueAllowChatParticipants)
            {
                rules.Add(new Data.PrivacyRule()
                {
                    PrivacyRuleType = PrivacyRuleType.AllowChatParticipants,
                    Peers = ((InputPrivacyValueAllowChatParticipantsImpl)r).Chats
                });
            }
            else if (r.Constructor == TLConstructor.InputPrivacyValueDisallowChatParticipants)
            {
                rules.Add(new Data.PrivacyRule()
                {
                    PrivacyRuleType = PrivacyRuleType.DisallowChatParticipants,
                    Peers = ((InputPrivacyValueDisallowChatParticipantsImpl)r).Chats
                });
            }
        }

        return rules;
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
        buff.Skip(4); _rules  =  factory . Read < Vector < InputPrivacyRule > > ( ref  buff ) ; 
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}