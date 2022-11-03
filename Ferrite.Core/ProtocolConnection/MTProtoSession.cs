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

using System.Buffers;
using System.Security.Cryptography;
using Ferrite.Crypto;
using Ferrite.Data;
using Ferrite.Services;
using Ferrite.TL;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.Core;

public class MTProtoSession
{
    private readonly IMTProtoService _mtproto;
    private readonly ILogger _log;
    private readonly IMTProtoTime _time;
    private readonly ISessionService _sessionService;
    private readonly IRandomGenerator _random;
    private readonly ITLObjectFactory _factory;
    private long _authKeyId;
    private long _permAuthKeyId;
    private byte[]? _authKey;
    private long _sessionId;
    private long _uniqueSessionId;
    private ServerSaltDTO _serverSalt = new ServerSaltDTO();
    private int _seq = 0;
    private long _lastMessageId;
    private readonly CircularQueue<long> _lastMessageIds = new CircularQueue<long>(10);
    private Dictionary<string, object> _sessionData = new();

    public MTProtoSession(IMTProtoService mtproto, ILogger log, ITLObjectFactory factory,
        IMTProtoTime time, ISessionService sessionService, IRandomGenerator random)
    {
        _mtproto = mtproto;
        _log = log;
        _factory = factory;
        _time = time;
        _sessionService = sessionService;
        _random = random;
    }
    
    public long AuthKeyId => _authKeyId;
    public long PermAuthKeyId => _permAuthKeyId;
    public byte[]? AuthKey => _authKey;
    public long SessionId => _sessionId;
    public long UniqueSessionId => _uniqueSessionId;
    public ServerSaltDTO ServerSalt => _serverSalt;
    public Dictionary<string, object> SessionData => _sessionData;

    public bool TryFetchAuthKey(long authKeyId)
    {
        if (Interlocked.CompareExchange(
                ref _authKeyId,
                authKeyId,
                0)
            != 0) return false;
        
        var authKey = _mtproto.GetAuthKey(_authKeyId);
        if (authKey != null)
        {
            _permAuthKeyId = _authKeyId;
            _log.Information($"Fetched the authKey with Id: {_authKeyId}");
        }
        else
        {
            authKey = _mtproto.GetTempAuthKey(_authKeyId);
            TryGetPermAuthKeyId();
            _log.Information($"Fetched the tempAuthKey with Id: {_authKeyId}");
        }

        if (authKey is { Length: 192 })
        {
            _authKey = authKey;
        }
        else
        {
            _authKeyId = 0;
            _permAuthKeyId = 0;
        }

        return _authKey != null;
    }

    private bool TryGetPermAuthKeyId()
    {
        if (_authKeyId == 0 || _permAuthKeyId != 0) return false;
        var pKey = _mtproto.GetBoundAuthKey(_authKeyId);
        _permAuthKeyId = pKey ?? 0;
        if (_permAuthKeyId != 0)
        {
            _log.Information($"Retrieved the permAuthKeyId: {_permAuthKeyId}");
        }
        return _permAuthKeyId != 0;
    }

    public int GenerateQuickAck(Span<byte> messageSpan)
    {
        var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        sha256.AppendData(_authKey.AsSpan().Slice(88, 32));
        sha256.AppendData(messageSpan);
        var ack = sha256.GetCurrentHash();
        return BitConverter.ToInt32(ack, 0);
    }

    public int GenerateSeqNo(bool isContentRelated)
    {
        return isContentRelated ? (2 * _seq++) + 1 : 2 * _seq;
    }
    /// <summary>
    /// Gets the next Message Identifier (msg_id) for this session.
    /// </summary>
    /// <param name="response">If the message is a response to a client message.</param>
    /// <returns></returns>
    public long NextMessageId(bool response)
    {
        long id = _time.GetUnixTimeInSeconds();
        id *= 4294967296L;
        long r1 = (4 - id % 4) % 4;
        id += (response ? r1 + 1 : r1 + 3);
        long last = _lastMessageId;
        long r2 = 4 - (last + 1) % 4;
        if (id <= last)
        {
            id = Interlocked.Add(ref _lastMessageId,
                response ? r2 + 2 : r2 + 4);
            if ((response && id % 4 == 1) || (!response && id % 4 == 3))
            {
                return id;
            }
        }
        else if (Interlocked.CompareExchange(ref _lastMessageId, id, last) == last)
        {
            return id;
        }
        do
        {
            r2 = 4 - (_lastMessageId + 1) % 4;
            id = Interlocked.Add(ref _lastMessageId, response ? r2 + 2 : r2 + 4);
        } while (!((response && id % 4 != 1) || (!response && id % 4 != 3)));
        return id;
    }
    public void CreateNewSession(long sessionId, long firstMessageId, MTProtoConnection connection)
    {
        _sessionId = sessionId;
        _uniqueSessionId = _random.NextLong();
        connection.SendNewSessionCreatedMessage(firstMessageId, 
            SaveCurrentSession(_permAuthKeyId != 0 ? 
                _permAuthKeyId : _authKeyId, connection));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="authKeyId"></param>
    /// <returns>Current Server Salt</returns>
    internal long SaveCurrentSession(long authKeyId, MTProtoConnection connection)
    {
        if (_serverSalt.ValidSince + 1800 < DateTimeOffset.Now.ToUnixTimeSeconds())
        {
            var salts = _mtproto.GetServerSalts(_permAuthKeyId, 1);
            if (salts != null)
            {
                foreach (var s in salts)
                {
                    if (s.ValidSince + 1800 <= _time.GetUnixTimeInSeconds()) continue;
                    _serverSalt = s;
                    break;
                }
            }
        }
        
        if (authKeyId != 0)
        {
            _sessionService.AddSession(authKeyId, _sessionId, 
                new ActiveSession(connection));
        }
        return _serverSalt.Salt;
    }
    /// <summary>
    /// Checks if the given message Id is valid and adds it to the last N messages list
    /// </summary>
    /// <param name="messageId"></param>
    /// <returns></returns>
    public bool IsValidMessageId(long messageId)
    {
        if (messageId >= _time.ThirtySecondsLater || //msg_id values that belong over 30 seconds in the future
            messageId <= _time.FiveMinutesAgo || //or over 300 seconds in the past are to be ignored
            messageId % 2 != 0 || //must have even parity
            (_lastMessageIds.Count != 0 && 
             (_lastMessageIds.Contains(messageId) || //must not be equal to any
              messageId <= _lastMessageIds.Min())) //must not be lower than all
            ) return false; 
        _lastMessageIds.Enqueue(messageId);
        return true;
    }
    
    public MTProtoMessage GenerateSessionCreated(long firstMessageId, long serverSalt)
    {
        var newSessionCreated = _factory.Resolve<NewSessionCreated>();
        newSessionCreated.FirstMsgId = firstMessageId;
        newSessionCreated.ServerSalt = serverSalt;
        newSessionCreated.UniqueId = UniqueSessionId;
        MTProtoMessage newSessionMessage = new()
        {
            Data = newSessionCreated.TLBytes.ToArray(),
            IsContentRelated = false,
            IsResponse = false,
            SessionId = SessionId,
            MessageType = MTProtoMessageType.NewSession
        };
        return newSessionMessage;
    }
}