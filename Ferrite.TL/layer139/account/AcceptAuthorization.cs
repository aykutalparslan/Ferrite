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

namespace Ferrite.TL.layer139.account;
public class AcceptAuthorization : ITLObject, ITLMethod
{
    private readonly SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
    private readonly ITLObjectFactory factory;
    private bool serialized = false;
    public AcceptAuthorization(ITLObjectFactory objectFactory)
    {
        factory = objectFactory;
    }

    public int Constructor => -202552205;
    public ReadOnlySequence<byte> TLBytes
    {
        get
        {
            if (serialized)
                return writer.ToReadOnlySequence();
            writer.Clear();
            writer.WriteInt32(Constructor, true);
            writer.WriteInt64(_botId, true);
            writer.WriteTLString(_scope);
            writer.WriteTLString(_publicKey);
            writer.Write(_valueHashes.TLBytes, false);
            writer.Write(_credentials.TLBytes, false);
            serialized = true;
            return writer.ToReadOnlySequence();
        }
    }

    private long _botId;
    public long BotId
    {
        get => _botId;
        set
        {
            serialized = false;
            _botId = value;
        }
    }

    private string _scope;
    public string Scope
    {
        get => _scope;
        set
        {
            serialized = false;
            _scope = value;
        }
    }

    private string _publicKey;
    public string PublicKey
    {
        get => _publicKey;
        set
        {
            serialized = false;
            _publicKey = value;
        }
    }

    private Vector<SecureValueHash> _valueHashes;
    public Vector<SecureValueHash> ValueHashes
    {
        get => _valueHashes;
        set
        {
            serialized = false;
            _valueHashes = value;
        }
    }

    private SecureCredentialsEncrypted _credentials;
    public SecureCredentialsEncrypted Credentials
    {
        get => _credentials;
        set
        {
            serialized = false;
            _credentials = value;
        }
    }

    public async Task<ITLObject> ExecuteAsync(TLExecutionContext ctx)
    {
        throw new NotImplementedException();
    }

    public void Parse(ref SequenceReader buff)
    {
        serialized = false;
        _botId = buff.ReadInt64(true);
        _scope = buff.ReadTLString();
        _publicKey = buff.ReadTLString();
        buff.Skip(4); _valueHashes  =  factory . Read < Vector < SecureValueHash > > ( ref  buff ) ; 
        buff.Skip(4); _credentials  =  factory . Read < SecureCredentialsEncrypted > ( ref  buff ) ; 
    }

    public void WriteTo(Span<byte> buff)
    {
        TLBytes.CopyTo(buff);
    }
}