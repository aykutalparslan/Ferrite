//
//    Project Ferrite is an Implementation Telegram Server API
//    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Affero General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using System.Security.Cryptography;

namespace Ferrite.Crypto;

public readonly ref struct AesIge
{
    //TODO: Can we reuse this somehow?
    private readonly Aes _aes;
    private readonly Span<byte> _aesIV;
    public AesIge(Span<byte> authKey, Span<byte> messageKey, bool fromClient = true)
    {
        int x = 0;
        if (!fromClient)
        {
            x = 8;
        }
        _aes = Aes.Create();
        Span<byte> tmp = stackalloc byte[52];
        Span<byte> sha256a = stackalloc byte[32];
        Span<byte> sha256b = stackalloc byte[32];
        messageKey.CopyTo(tmp);
        authKey.Slice(0+x, 36).CopyTo(tmp.Slice(16));
        SHA256.HashData(tmp, sha256a);
        tmp.Clear();
        authKey.Slice(40+x, 36).CopyTo(tmp);
        messageKey.CopyTo(tmp.Slice(36));
        SHA256.HashData(tmp, sha256b);
        var _aesKey = new byte[32];
        _aesIV = new byte[32];
        sha256a.Slice(0, 8).CopyTo(_aesKey);
        sha256b.Slice(8, 16).CopyTo(_aesKey.AsSpan().Slice(8));
        sha256a.Slice(24, 8).CopyTo(_aesKey.AsSpan().Slice(24));
        sha256b.Slice(0, 8).CopyTo(_aesIV);
        sha256a.Slice(8, 16).CopyTo(_aesIV.Slice(8));
        sha256b.Slice(24, 8).CopyTo(_aesIV.Slice(24));
        _aes.Key = _aesKey;
    }

    public void Decrypt(Span<byte> message)
    {
        _aes.DecryptIge(message, _aesIV);
    }

    public void Decrypt(Span<byte> source, Span<byte> destination)
    {
        _aes.DecryptIge(source, _aesIV, destination);
    }

    public static Span<byte> GenerateMessageKey(Span<byte> authKey, Span<byte> plaintext, bool fromClient = false)
    {
        int x = 0;
        if (!fromClient)
        {
            x = 8;
        }
        var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        sha256.AppendData(authKey.Slice(88+x, 32));
        sha256.AppendData(plaintext);
        Span<byte> messageKeyLarge = sha256.GetCurrentHash();
        Span<byte> tmp = new byte[32 + plaintext.Length];
        SHA256.HashData(tmp, messageKeyLarge);
        Span<byte> messageKey = messageKeyLarge.Slice(8);
        return messageKey;
    }
}

