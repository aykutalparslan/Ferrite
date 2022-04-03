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
using Ferrite.Services;
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.layer139.langpack;
public class GetStrings : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly ILangPackService _langPackService;
    private const string Zero = "_zero";
    private const string One = "_one";
    private const string Two = "_two";
    private const string Few = "_few";
    private const string Many = "_many";
    private const string Other = "_other";
    private bool serialized = false;
    public GetStrings(ITLObjectFactory objectFactory, ILangPackService langPackService)
    {
        factory = objectFactory;
        _langPackService = langPackService;
    }

    public int Constructor => -269862909;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_langPack);
            writer.WriteTLString(_langCode);
            writer.Write(_keys.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
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

    private VectorOfString _keys;
    public VectorOfString Keys
    {
        get => _keys;
        set
        {
            serialized = false;
            _keys = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        Vector<LangPackString> strings = factory.Resolve<Vector<LangPackString>>();
        var tmp = await _langPackService.GetStringsAsync(_langPack, _langCode, _keys);
        foreach (var key in _keys)
        {
            if (tmp!= null && tmp.ContainsKey(key))
            {
                LangPackStringImpl str = factory.Resolve<LangPackStringImpl>();
                str.Key = key;
                str.Value = tmp[key];
                strings.Add(str);
            }
            else if (tmp != null && (tmp.ContainsKey(key + Zero) ||
                tmp.ContainsKey(key + One) || tmp.ContainsKey(key + Two) ||
                tmp.ContainsKey(key + Few) || tmp.ContainsKey(key + Many) ||
                tmp.ContainsKey(key + Other)))
            {
                LangPackStringPluralizedImpl str = factory.Resolve<LangPackStringPluralizedImpl>();
                str.Key = key;
                if (tmp.ContainsKey(key + Zero))
                {
                    str.ZeroValue = tmp[key + Zero];
                }
                if (tmp.ContainsKey(key + One))
                {
                    str.OneValue = tmp[key + One];
                }
                if (tmp.ContainsKey(key + Two))
                {
                    str.TwoValue = tmp[key + Two];
                }
                if (tmp.ContainsKey(key + Few))
                {
                    str.FewValue = tmp[key + Few];
                }
                if (tmp.ContainsKey(key + Many))
                {
                    str.ManyValue = tmp[key + Many];
                }
                if (tmp.ContainsKey(key + Other))
                {
                    str.OtherValue = tmp[key + Other];
                }
                strings.Add(str);
            }
        }
        RpcResult result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        result.Result = strings;
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _langPack = buff.ReadTLString();
        _langCode = buff.ReadTLString();
        buff.Skip(4); _keys  =  factory . Read < VectorOfString > ( ref  buff ) ; 
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}