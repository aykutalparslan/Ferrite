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
    public interface IKeyProvider
    {
        public IList<long> GetRSAFingerprints();
        public IRSAKey? GetKey(long fingerprint);
    }
}

