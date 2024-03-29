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
using Ferrite.TL.mtproto;
using Ferrite.Utils;

namespace Ferrite.TL.currentLayer.help;
public class GetCountriesList : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public GetCountriesList(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => 1935116200;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteTLString(_langCode);
            writer.WriteInt32(_hash, true);
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

    private int _hash;
    public int Hash
    {
        get => _hash;
        set
        {
            serialized = false;
            _hash = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        var result = factory.Resolve<RpcResult>();
        result.ReqMsgId = ctx.MessageId;
        var countries = factory.Resolve<CountriesListImpl>();
        var countryList = factory.Resolve<Vector<Country>>();
        var tr = factory.Resolve<CountryImpl>();
        tr.Iso2 = "TUR";
        tr.Name = "Türkiye";
        tr.DefaultName = "Türkiye";
        var countryCodes = factory.Resolve<Vector<CountryCode>>();
        var code = factory.Resolve<CountryCodeImpl>();
        code.CountryCode = "TUR";
        code.Prefixes = new VectorOfString();
        code.Patterns = new VectorOfString();
        code.Prefixes.Add("+90");
        code.Prefixes.Add("0090");
        code.Patterns.Add("XXX XXX XX XX");
        countryCodes.Add(code);
        result.Result = countryCodes;
        return result;
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _langCode = buff.ReadTLString();
        _hash = buff.ReadInt32(true);
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}