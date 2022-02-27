/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
using System.Security.Cryptography;

namespace Ferrite.Crypto
{
    public interface IRSAKey
    {
        public RSA Key { get; }
        public RSAParameters PublicKeyParameters { get; }
        public RSAParameters PrivateKeyParameters { get; }
        public long Fingerprint { get; }
        public void Init(string alias);
        public byte[] EncryptBlock(byte[] data, bool usePublicKey = true);
        public byte[] DecryptBlock(byte[] data, bool usePrivateKey = true);
    }
}

