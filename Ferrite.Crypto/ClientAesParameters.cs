using System;
using System.Security.Cryptography;

namespace Ferrite.Crypto;

public readonly struct ClientAesParameters
{
    private readonly byte[] _aesKey;
    public byte[] AesKey => _aesKey;

    private readonly byte[] _aesIV;
    public byte[] AesIV => _aesIV;

    public ClientAesParameters(Span<byte> authKey, Span<byte> messageKey)
    {
        Span<byte> tmp = stackalloc byte[52];
        Span<byte> sha256a = stackalloc byte[32];
        Span<byte> sha256b = stackalloc byte[32];
        messageKey.CopyTo(tmp);
        authKey.Slice(0, 36).CopyTo(tmp.Slice(16));
        SHA256.HashData(tmp, sha256a);
        tmp.Clear();
        authKey.Slice(40, 36).CopyTo(tmp);
        messageKey.CopyTo(tmp.Slice(36));
        SHA256.HashData(tmp, sha256b);
        _aesKey = new byte[32];
        _aesIV = new byte[32];
        sha256a.Slice(0, 8).CopyTo(_aesKey);
        sha256b.Slice(8, 16).CopyTo(_aesKey.AsSpan().Slice(8));
        sha256a.Slice(24, 8).CopyTo(_aesKey.AsSpan().Slice(24));
        sha256b.Slice(0, 8).CopyTo(_aesIV);
        sha256a.Slice(8, 16).CopyTo(_aesIV.AsSpan().Slice(8));
        sha256b.Slice(24, 8).CopyTo(_aesIV.AsSpan().Slice(24));
    }
}

