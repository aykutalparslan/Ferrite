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
public class LangPackLanguageImpl : LangPackLanguage
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public LangPackLanguageImpl(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public override int Constructor => -288727837;
    public override ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.Write<Flags>(_flags);
            writer.WriteTLString(_name);
            writer.WriteTLString(_nativeName);
            writer.WriteTLString(_langCode);
            if (_flags[1])
            {
                writer.WriteTLString(_baseLangCode);
            }

            writer.WriteTLString(_pluralCode);
            writer.WriteInt32(_stringsCount, true);
            writer.WriteInt32(_translatedCount, true);
            writer.WriteTLString(_translationsUrl);
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

    public bool Official
    {
        get => _flags[0];
        set
        {
            serialized = false;
            _flags[0] = value;
        }
    }

    public bool Rtl
    {
        get => _flags[2];
        set
        {
            serialized = false;
            _flags[2] = value;
        }
    }

    public bool Beta
    {
        get => _flags[3];
        set
        {
            serialized = false;
            _flags[3] = value;
        }
    }

    private string _name;
    public string Name
    {
        get => _name;
        set
        {
            serialized = false;
            _name = value;
        }
    }

    private string _nativeName;
    public string NativeName
    {
        get => _nativeName;
        set
        {
            serialized = false;
            _nativeName = value;
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

    private string _baseLangCode;
    public string BaseLangCode
    {
        get => _baseLangCode;
        set
        {
            serialized = false;
            _flags[1] = true;
            _baseLangCode = value;
        }
    }

    private string _pluralCode;
    public string PluralCode
    {
        get => _pluralCode;
        set
        {
            serialized = false;
            _pluralCode = value;
        }
    }

    private int _stringsCount;
    public int StringsCount
    {
        get => _stringsCount;
        set
        {
            serialized = false;
            _stringsCount = value;
        }
    }

    private int _translatedCount;
    public int TranslatedCount
    {
        get => _translatedCount;
        set
        {
            serialized = false;
            _translatedCount = value;
        }
    }

    private string _translationsUrl;
    public string TranslationsUrl
    {
        get => _translationsUrl;
        set
        {
            serialized = false;
            _translationsUrl = value;
        }
    }

    public override void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _flags = buff.Read<Flags>();
        _name = buff.ReadTLString();
        _nativeName = buff.ReadTLString();
        _langCode = buff.ReadTLString();
        if (_flags[1])
        {
            _baseLangCode = buff.ReadTLString();
        }

        _pluralCode = buff.ReadTLString();
        _stringsCount = buff.ReadInt32(true);
        _translatedCount = buff.ReadInt32(true);
        _translationsUrl = buff.ReadTLString();
    }

    public override void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}