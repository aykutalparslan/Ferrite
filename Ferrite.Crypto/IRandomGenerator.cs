/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using System;
using System.Numerics;

namespace Ferrite.Crypto
{
    public interface IRandomGenerator
    {
        public int GetRandomPrime();
        public int GetRandomNumber(int toExclusive);
        public int GetRandomNumber(int fromInclusive, int toExclusive);
        public byte[] GetRandomBytes(int count);
        public BigInteger GetRandomInteger(BigInteger min, BigInteger max);
    }
}

