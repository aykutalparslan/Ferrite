﻿/*
 *   Project Ferrite is an Implementation Telegram Server API
 *   Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
 *
 *   This program is free software: you can redistribute it and/or modify
 *   it under the terms of the GNU Affero General Public License as published by
 *   the Free Software Foundation, either version 3 of the License, or
 *   (at your option) any later version.
 *
 *   This program is distributed in the hope that it will be useful,
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *   GNU Affero General Public License for more details.
 *
 *   You should have received a copy of the GNU Affero General Public License
 *   along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using Xunit;
using Ferrite.Crypto;
using System.Linq;
using System;
using System.Security.Cryptography;

namespace Ferrite.Tests.Crypto;
// Test data from OpenSSL IGE Paper https://www.links.org/files/openssl-ige.pdf
public class IgeExtensions
{
    public static byte[] StringToByteArray(string hex)
    {
        return Enumerable.Range(0, hex.Length)
                         .Where(x => x % 2 == 0)
                         .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                         .ToArray();
    }

    [Fact]
    public void Encrypt_ShouldProduceCipherText()
    {
        var tmp_aes_key = "000102030405060708090A0B0C0D0E0F";
        var tmp_aes_iv = "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F";
        var encrypted_answer = "0000000000000000000000000000000000000000000000000000000000000000";
        var answer = "1A8519A6557BE652E9DA8E43DA4EF4453CF456B4CA488AA383C79C98B34797CB";

        byte[] plaintext = StringToByteArray(encrypted_answer);
        byte[] key = StringToByteArray(tmp_aes_key);
        byte[] iv = StringToByteArray(tmp_aes_iv);
        byte[] ciphertext = StringToByteArray(answer);

        Aes aes = Aes.Create();
        aes.Key = key;
        byte[] result = new byte[plaintext.Length];
        aes.EncryptIge(plaintext, iv, result);

        Assert.Equal<byte[]>(ciphertext, result);
    }

    [Fact]
    public void Encrypt_ShouldEncryptInPlace()
    {
        var tmp_aes_key = "000102030405060708090A0B0C0D0E0F";
        var tmp_aes_iv = "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F";
        var encrypted_answer = "0000000000000000000000000000000000000000000000000000000000000000";
        var answer = "1A8519A6557BE652E9DA8E43DA4EF4453CF456B4CA488AA383C79C98B34797CB";

        byte[] plaintext = StringToByteArray(encrypted_answer);
        byte[] key = StringToByteArray(tmp_aes_key);
        byte[] iv = StringToByteArray(tmp_aes_iv);
        byte[] ciphertext = StringToByteArray(answer);
        Aes aes = Aes.Create();
        aes.Key = key;
        aes.EncryptIge(plaintext,iv);

        Assert.Equal<byte[]>(ciphertext, plaintext);
    }

    [Fact]
    public void Decrypt_ShouldProducePlaintext()
    {
        var tmp_aes_key = "000102030405060708090A0B0C0D0E0F";
        var tmp_aes_iv = "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F";
        var encrypted_answer = "0000000000000000000000000000000000000000000000000000000000000000";
        var answer = "1A8519A6557BE652E9DA8E43DA4EF4453CF456B4CA488AA383C79C98B34797CB";

        byte[] plaintext = StringToByteArray(encrypted_answer);
        byte[] key = StringToByteArray(tmp_aes_key);
        byte[] iv = StringToByteArray(tmp_aes_iv);
        byte[] ciphertext = StringToByteArray(answer);

        Aes aes = Aes.Create();
        aes.Key = key;
        byte[] result = new byte[plaintext.Length];
        aes.DecryptIge(ciphertext, iv, result);

        Assert.Equal<byte[]>(plaintext, result);
    }

    [Fact]
    public void Decrypt_ShouldDecryptInPlace()
    {
        var tmp_aes_key = "000102030405060708090A0B0C0D0E0F";
        var tmp_aes_iv = "000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F";
        var encrypted_answer = "0000000000000000000000000000000000000000000000000000000000000000";
        var answer = "1A8519A6557BE652E9DA8E43DA4EF4453CF456B4CA488AA383C79C98B34797CB";

        byte[] plaintext = StringToByteArray(encrypted_answer);
        byte[] key = StringToByteArray(tmp_aes_key);
        byte[] iv = StringToByteArray(tmp_aes_iv);
        byte[] ciphertext = StringToByteArray(answer);

        Aes aes = Aes.Create();
        aes.Key = key;
        aes.DecryptIge(ciphertext, iv);

        Assert.Equal<byte[]>(plaintext, ciphertext);
    }
}
