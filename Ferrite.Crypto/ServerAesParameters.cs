using System;
using System.Security.Cryptography;

namespace Ferrite.Crypto;

public readonly struct ServerAesParameters : AesParameters
{
    private readonly byte[] _aesKey;
    public byte[] AesKey => _aesKey;

    private readonly byte[] _aesIV;
    public byte[] AesIV => _aesIV;

    private readonly byte[] _messageKey;
    public byte[] MessageKey => _messageKey;

    public ServerAesParameters(Span<byte> authKey, Span<byte> plaintext)
    {
        var sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        sha256.AppendData(authKey.Slice(96, 32));
        sha256.AppendData(plaintext);
        Span<byte> messageKeyLarge = sha256.GetCurrentHash();
        Span<byte> tmp = new byte[32+plaintext.Length];
        SHA256.HashData(tmp, messageKeyLarge);
        Span<byte> messageKey = messageKeyLarge.Slice(8);
        Span<byte> sha256a = stackalloc byte[32];
        Span<byte> sha256b = stackalloc byte[32];
        tmp = tmp.Slice(0, 52);
        tmp.Clear();
        messageKey.CopyTo(tmp);
        authKey.Slice(8, 36).CopyTo(tmp.Slice(16));
        
        SHA256.HashData(tmp, sha256a);
        tmp.Clear();
        authKey.Slice(48, 36).CopyTo(tmp);
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
        _messageKey = messageKey.ToArray();
    }
}
