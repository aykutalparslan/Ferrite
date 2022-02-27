/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;

namespace Ferrite.Crypto
{
    public class KeyProvider :IKeyProvider
    {
        private Dictionary<long, IRSAKey> keyPairs = new();
        public KeyProvider()
        {
            RSAKey keyPair = new RSAKey();
            keyPair.Init("default");
            keyPairs.Add(keyPair.Fingerprint, keyPair);
        }

        public IList<long> GetRSAFingerprints()
        {
            return keyPairs.Keys.ToList();
        }

        public IRSAKey? GetKey(long fingerprint)
        {
            if(keyPairs.TryGetValue(fingerprint, out var keyPair))
            {
                return keyPair;
            }
            return null;
        }
    }
}

