/*
 *    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *    This file is a part of Project Ferrite
 *
 *    Proprietary and confidential.
 *    Copying without express written permission is strictly prohibited.
 */

using Xunit;
using System;
using Ferrite.Crypto;
using System.Collections.Generic;

namespace Ferrite.Tests.Crypto;

//Test data taken from: https://en.wikipedia.org/wiki/List_of_prime_numbers
public class RandomPrimeGeneratorTests
{
    
    [Fact]
    public void IsProbablyPrime_ShouldReturnTrue()
    {
        int[] primes = new int[] { 6143, 6151, 6163, 6173, 6197, 6199, 6203, 6211,
            6217, 6221, 6229, 6247, 6257, 6263, 6269, 6271, 6277, 6287, 6299, 6301};

        for (int i = 0; i < primes.Length; i++)
        {
            Assert.True(RandomGenerator.MillerRabin(primes[i]));
        }
    }
    [Fact]
    public void IsProbablyPrime_ShouldReturnFalse()
    {
        int[] nonPrimes = new int[] { 6142, 6153, 6161, 6171, 6191, 6193, 6201, 6213,
            6215, 6223, 6227, 6246, 6254, 6261, 6267, 6273, 6275, 6285, 6290, 6300};

        for (int i = 0; i < nonPrimes.Length; i++)
        {
            Assert.False(RandomGenerator.MillerRabin(nonPrimes[i]));
        }
    }
    [Fact]
    public void SieveOfErasthosthenes_ShouldReturnFirst20Primes()
    {
        int[] expected = new int[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71 };
        int[] val = RandomGenerator.SieveOfEratosthenes(71);
        Assert.Equal(expected, val);
    }

    [Fact]
    public void SieveOfErasthosthenesSegmented_ShouldReturnPrimesInRange()
    {
        int[] primes = RandomGenerator.SieveOfEratosthenes(7529);
        List<int> range = new();
        foreach (var item in primes)
        {
            //if(item>=1087 && item <= 7529)
            if (item >= 2 && item <= 50)
            {
                range.Add(item);
            }
        }
        var val = RandomGenerator.SieveOfEratosthenesSegmented(2, 50);
        range = new();
        foreach (var item in primes)
        {
            if(item>=1087 && item <= 7529)
            {
                range.Add(item);
            }
        }
        val = RandomGenerator.SieveOfEratosthenesSegmented(1087, 7529);
        Assert.Equal(range.ToArray(), val); ;
    }

    [Fact]
    public void GetRandomPrime_ShouldReturnPrime()
    {
        RandomGenerator? rpg = new RandomGenerator();
        int p =  rpg.GetRandomPrime();
        
        Assert.True(RandomGenerator.MillerRabin(p));
    }
}
