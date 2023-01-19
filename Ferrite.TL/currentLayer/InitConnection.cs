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
using Ferrite.Crypto;
using Ferrite.Services;
using Ferrite.TL.Exceptions;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer;
public class InitConnection : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ILogger _log;
    private readonly ITLObjectFactory factory;
    private readonly IAuthService _auth;
    private readonly IRandomGenerator _random;
    private bool serialized = false;
    public InitConnection(ITLObjectFactory objectFactory, ILogger logger, IAuthService auth, IRandomGenerator random)
    {
        factory = objectFactory;
        _log = logger;
        _auth = auth;
        _random = random;
    }

    public int Constructor => -1043505495;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteInt32(_apiId, true);
            writer.WriteTLString(_deviceModel);
            writer.WriteTLString(_systemVersion);
            writer.WriteTLString(_appVersion);
            writer.WriteTLString(_systemLangCode);
            writer.WriteTLString(_langPack);
            writer.WriteTLString(_langCode);
            if (_flags[0])
            {
                writer.Write(_proxy.TLBytes, false);
            }

            if (_flags[1])
            {
                writer.Write(_params.TLBytes, false);
            }

            writer.Write(_query.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private Flags _flags;
    public Flags Flags
    {
        get => _flags;
        set
        {
            serialized = false;
            _flags = value;
        }
    }

    private int _apiId;
    public int ApiId
    {
        get => _apiId;
        set
        {
            serialized = false;
            _apiId = value;
        }
    }

    private string _deviceModel;
    public string DeviceModel
    {
        get => _deviceModel;
        set
        {
            serialized = false;
            _deviceModel = value;
        }
    }

    private string _systemVersion;
    public string SystemVersion
    {
        get => _systemVersion;
        set
        {
            serialized = false;
            _systemVersion = value;
        }
    }

    private string _appVersion;
    public string AppVersion
    {
        get => _appVersion;
        set
        {
            serialized = false;
            _appVersion = value;
        }
    }

    private string _systemLangCode;
    public string SystemLangCode
    {
        get => _systemLangCode;
        set
        {
            serialized = false;
            _systemLangCode = value;
        }
    }

    private string _langPack;
    public string LangPack
    {
        get => _langPack;
        set
        {
            serialized = false;
            _langPack = value;
        }
    }

    private string _langCode;
    public string LangCode
    {
        get => _langCode;
        set
        {
            serialized = false;
            _langCode = value;
        }
    }

    private InputClientProxy _proxy;
    public InputClientProxy Proxy
    {
        get => _proxy;
        set
        {
            serialized = false;
            _flags[0] = true;
            _proxy = value;
        }
    }

    private JSONValue _params;
    public JSONValue Params
    {
        get => _params;
        set
        {
            serialized = false;
            _flags[1] = true;
            _params = value;
        }
    }

    private ITLObject _query;
    public ITLObject Query
    {
        get => _query;
        set
        {
            serialized = false;
            _query = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        /*_ = _auth.SaveAppInfo(new Data.AppInfoDTO()
        {
            Hash = _random.NextLong(),
            ApiId = _apiId,
            AppVersion = _appVersion,
            AuthKeyId = ctx.PermAuthKeyId!=0 ? ctx.PermAuthKeyId : ctx.AuthKeyId,
            DeviceModel = _deviceModel,
            IP = ctx.IP,
            LangCode = _langCode,
            LangPack = _langPack,
            SystemLangCode = _systemLangCode,
            SystemVersion = _systemVersion
        });*/
        //if ((int)ctx.SessionData["layer"] != 139)
        //{
        //    var err = factory.Resolve<RpcError>();
        //    err.ErrorCode = 400;
        //    err.ErrorMessage = "CONNECTION_LAYER_INVALID";
        //    return err;
        //}
        if (_query is ITLMethod medhod)
        {
            _log.Information(String.Format("Execute {0}", medhod.ToString()));
            return await medhod.ExecuteAsync(ctx);
        }
        //var ack = factory.Resolve<MsgsAck>();
        //ack.MsgIds = new VectorOfLong(1);
        //ack.MsgIds.Add(ctx.MessageId);
        //return ack;
        return null;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _apiId = buff.ReadInt32(true);
        _deviceModel = buff.ReadTLString();
        _systemVersion = buff.ReadTLString();
        _appVersion = buff.ReadTLString();
        _systemLangCode = buff.ReadTLString();
        _langPack = buff.ReadTLString();
        _langCode = buff.ReadTLString();
        if (_flags[0])
        {
            _proxy = (InputClientProxy)factory.Read(buff.ReadInt32(true), ref buff);
        }

        if (_flags[1])
        {
            _params = (JSONValue)factory.Read(buff.ReadInt32(true), ref buff);
        }

        _query = factory.Read(buff.ReadInt32(true), ref buff);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}