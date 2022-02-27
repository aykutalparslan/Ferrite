/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
using System.Buffers;
using System.Security.Cryptography;
using DotNext.Buffers;
using Ferrite.Utils;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;

namespace Ferrite.Crypto;

public class RSAKey : IRSAKey
{
    private RSA key;
    public RSA Key => key;
    private RSAParameters privateKeyParameters;
    public RSAParameters PrivateKeyParameters => privateKeyParameters;
    private RSAParameters publicKeyParameters;
    public RSAParameters PublicKeyParameters => publicKeyParameters;
    private long fingerprint;
    public long Fingerprint => fingerprint;
    private ICipherParameters publicKey;
    private ICipherParameters privateKey;

    private void CalculateFingerprint()
    {
        SparseBufferWriter<byte> writer = new SparseBufferWriter<byte>(UnmanagedMemoryPool<byte>.Shared);
        writer.WriteTLBytes(publicKeyParameters.Modulus);
        writer.WriteTLBytes(publicKeyParameters.Exponent);
        var sha1 = SHA1.HashData(writer.ToReadOnlySequence().ToArray());
        fingerprint = BitConverter.ToInt64(sha1, 12);
    }
    private RSA GetRSAKeyPair(string alias)
    {
        RSA rsa = RSA.Create(2048);
        string[] keyFiles = new string[] { alias + "-private.key", alias + "-public.key" };
        bool exists = true;
        foreach (var filename in keyFiles)
        {
            exists = File.Exists(filename);
        }
        if (!exists)
        {
            File.WriteAllBytes(alias + "-private.key", rsa.ExportRSAPrivateKey());
            File.WriteAllBytes(alias + "-public.key", rsa.ExportRSAPrivateKey());
        }
        else
        {
            rsa.ImportRSAPrivateKey(File.ReadAllBytes(alias + "-private.key"),
                out var bytesRead);
        }
        return rsa;
    }

    public void Init(string alias)
    {
        key = GetRSAKeyPair(alias);
        publicKeyParameters = key.ExportParameters(false);
        privateKeyParameters = key.ExportParameters(true);
        CalculateFingerprint();
        publicKey = (ICipherParameters)new PemReader(new StringReader(ExportPublicKey())).ReadObject();
        privateKey = ((AsymmetricCipherKeyPair)new PemReader(new StringReader(ExportPrivateKey())).ReadObject()).Private;
    }

    // https://stackoverflow.com/a/28407693 -----------------------------------
    public string ExportPublicKey()
    {
        using (var stream = new MemoryStream())
        {
            var writer = new BinaryWriter(stream);
            writer.Write((byte)0x30); // SEQUENCE
            using (var innerStream = new MemoryStream())
            {
                var innerWriter = new BinaryWriter(innerStream);
                innerWriter.Write((byte)0x30); // SEQUENCE
                EncodeLength(innerWriter, 13);
                innerWriter.Write((byte)0x06); // OBJECT IDENTIFIER
                var rsaEncryptionOid = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01 };
                EncodeLength(innerWriter, rsaEncryptionOid.Length);
                innerWriter.Write(rsaEncryptionOid);
                innerWriter.Write((byte)0x05); // NULL
                EncodeLength(innerWriter, 0);
                innerWriter.Write((byte)0x03); // BIT STRING
                using (var bitStringStream = new MemoryStream())
                {
                    var bitStringWriter = new BinaryWriter(bitStringStream);
                    bitStringWriter.Write((byte)0x00); // # of unused bits
                    bitStringWriter.Write((byte)0x30); // SEQUENCE
                    using (var paramsStream = new MemoryStream())
                    {
                        var paramsWriter = new BinaryWriter(paramsStream);
                        EncodeIntegerBigEndian(paramsWriter, publicKeyParameters.Modulus); // Modulus
                        EncodeIntegerBigEndian(paramsWriter, publicKeyParameters.Exponent); // Exponent
                        var paramsLength = (int)paramsStream.Length;
                        EncodeLength(bitStringWriter, paramsLength);
                        bitStringWriter.Write(paramsStream.GetBuffer(), 0, paramsLength);
                    }
                    var bitStringLength = (int)bitStringStream.Length;
                    EncodeLength(innerWriter, bitStringLength);
                    innerWriter.Write(bitStringStream.GetBuffer(), 0, bitStringLength);
                }
                var length = (int)innerStream.Length;
                EncodeLength(writer, length);
                writer.Write(innerStream.GetBuffer(), 0, length);
            }

            var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length);
            return "-----BEGIN PUBLIC KEY-----\n" +
            base64 + "\n" +
            "-----END PUBLIC KEY-----\n";
        }
    }

    private static void EncodeLength(BinaryWriter stream, int length)
    {
        if (length < 0) throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
        if (length < 0x80)
        {
            // Short form
            stream.Write((byte)length);
        }
        else
        {
            // Long form
            var temp = length;
            var bytesRequired = 0;
            while (temp > 0)
            {
                temp >>= 8;
                bytesRequired++;
            }
            stream.Write((byte)(bytesRequired | 0x80));
            for (var i = bytesRequired - 1; i >= 0; i--)
            {
                stream.Write((byte)(length >> (8 * i) & 0xff));
            }
        }
    }

    private static void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
    {
        stream.Write((byte)0x02); // INTEGER
        var prefixZeros = 0;
        for (var i = 0; i < value.Length; i++)
        {
            if (value[i] != 0) break;
            prefixZeros++;
        }
        if (value.Length - prefixZeros == 0)
        {
            EncodeLength(stream, 1);
            stream.Write((byte)0);
        }
        else
        {
            if (forceUnsigned && value[prefixZeros] > 0x7f)
            {
                // Add a prefix zero to force unsigned if the MSB is 1
                EncodeLength(stream, value.Length - prefixZeros + 1);
                stream.Write((byte)0);
            }
            else
            {
                EncodeLength(stream, value.Length - prefixZeros);
            }
            for (var i = prefixZeros; i < value.Length; i++)
            {
                stream.Write(value[i]);
            }
        }
    }
    // https://stackoverflow.com/a/28407693/ -----------------------------------

    // https://stackoverflow.com/a/23739932/ -----------------------------------
    public string ExportPrivateKey()
    {
        using (var stream = new MemoryStream())
        {
            var writer = new BinaryWriter(stream);
            writer.Write((byte)0x30); // SEQUENCE
            using (var innerStream = new MemoryStream())
            {
                var innerWriter = new BinaryWriter(innerStream);
                EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version
                EncodeIntegerBigEndian(innerWriter, privateKeyParameters.Modulus);
                EncodeIntegerBigEndian(innerWriter, privateKeyParameters.Exponent);
                EncodeIntegerBigEndian(innerWriter, privateKeyParameters.D);
                EncodeIntegerBigEndian(innerWriter, privateKeyParameters.P);
                EncodeIntegerBigEndian(innerWriter, privateKeyParameters.Q);
                EncodeIntegerBigEndian(innerWriter, privateKeyParameters.DP);
                EncodeIntegerBigEndian(innerWriter, privateKeyParameters.DQ);
                EncodeIntegerBigEndian(innerWriter, privateKeyParameters.InverseQ);
                var length = (int)innerStream.Length;
                EncodeLength(writer, length);
                writer.Write(innerStream.GetBuffer(), 0, length);
            }

            var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length);
            return "-----BEGIN RSA PRIVATE KEY-----\n"+
            base64 + "\n"+
            "-----END RSA PRIVATE KEY-----\n";
        }
    }

    // https://stackoverflow.com/a/23739932/ -----------------------------------

    public byte[] EncryptBlockWithPublicKey(byte[] data)
    {
        if (data.Length != 256)
        {
            throw new ArgumentOutOfRangeException();
        }
        var engine = new RsaEngine();
        string pkey = ExportPublicKey();
        var parameters = (AsymmetricKeyParameter)new PemReader(new StringReader(pkey)).ReadObject();


        engine.Init(true, parameters);
        return engine.ProcessBlock(data, 0, data.Length);
    }

    public byte[] EncryptBlock(byte[] data, bool usePublicKey = true)
    {
        if (data.Length != 256)
        {
            throw new ArgumentOutOfRangeException();
        }
        var engine = new RsaEngine();

        engine.Init(true, usePublicKey ? publicKey:privateKey);
        return engine.ProcessBlock(data, 0, 256);
    }

    public byte[] DecryptBlock(byte[] data, bool usePrivateKey = true)
    {
        if (data.Length != 256)
        {
            throw new ArgumentOutOfRangeException();
        }
        var engine = new RsaEngine();

        engine.Init(false, usePrivateKey ? privateKey : publicKey);
        return engine.ProcessBlock(data, 0, 256);
    }
}