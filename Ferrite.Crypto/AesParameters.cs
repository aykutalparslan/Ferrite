using System;
namespace Ferrite.Crypto;

public interface AesParameters
{
    public byte[] AesKey { get; }
    public byte[] AesIV { get; }
    public byte[] MessageKey { get; }
}