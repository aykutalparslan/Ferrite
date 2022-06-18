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

namespace Ferrite.TL.currentLayer.langpack;
public class GetLangPackL67 : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private readonly ILangPackService _langPackService;
    private bool serialized = false;
    public GetLangPackL67(ITLObjectFactory objectFactory, ILangPackService langPackService)
    {
        factory = objectFactory;
        _langPackService = langPackService;
    }

    public int Constructor => unchecked((int)0x9ab5c58e);
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_langCode);
            serialized = true;
            return writer.ToReadOnlySequence();
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

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        RpcResult result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var difference = await _langPackService.GetLangPackAsync("android", _langCode);
        var langPack = factory.Resolve<LangPackDifferenceImpl>();
        langPack.LangCode = difference.LangCode;
        langPack.FromVersion = difference.Version;
        langPack.Version = difference.Version;
        Vector<LangPackString> strings = factory.Resolve<Vector<LangPackString>>();
        foreach (var langPackString in difference.Strings)
        {
            if (langPackString.StringType == LangPackStringType.Default)
            {
                LangPackStringImpl str = factory.Resolve<LangPackStringImpl>();
                str.Key = langPackString.Key;
                str.Value = langPackString.Value;
                strings.Add(str);
            }
            else if (langPackString.StringType == LangPackStringType.Pluralized)
            {
                LangPackStringPluralizedImpl str = factory.Resolve<LangPackStringPluralizedImpl>();
                str.Key = langPackString.Key;
                if (langPackString.ZeroValue != null && langPackString.ZeroValue.Length > 0)
                {
                    str.ZeroValue = langPackString.ZeroValue;
                }
                if (langPackString.OneValue != null && langPackString.OneValue.Length > 0)
                {
                    str.OneValue = langPackString.OneValue;
                }
                if (langPackString.TwoValue != null && langPackString.TwoValue.Length > 0)
                {
                    str.TwoValue = langPackString.TwoValue;
                }
                if (langPackString.FewValue != null && langPackString.FewValue.Length > 0)
                {
                    str.FewValue = langPackString.FewValue;
                }
                if (langPackString.ManyValue != null && langPackString.ManyValue.Length > 0)
                {
                    str.ManyValue = langPackString.ManyValue;
                }
                if (langPackString.OtherValue != null && langPackString.OtherValue.Length > 0)
                {
                    str.OtherValue = langPackString.OtherValue;
                }
                strings.Add(str);
            }
        }

        langPack.Strings = strings;
        result.Result = langPack;
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _langCode = buff.ReadTLString();
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}