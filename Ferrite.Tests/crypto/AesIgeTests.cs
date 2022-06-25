//
//  Project Ferrite is an Implementation Telegram Server API
//  Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Affero General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Affero General Public License for more details.
//
//  You should have received a copy of the GNU Affero General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using System.IO;
using Ferrite.Crypto;
using Xunit;

namespace Ferrite.Tests.Crypto
{
    public class AesIgeTests
    {
        [Fact]
        public void ShouldGenerateClientMessageKey()
        {
            byte[] authKey = File.ReadAllBytes("testdata/crypto/authKeyClient");
            byte[] messageKey = File.ReadAllBytes("testdata/crypto/messageKeyClient");
            byte[] plaintext = File.ReadAllBytes("testdata/crypto/messageClientDecrypted");
            var messageKeyActual = AesIge.GenerateMessageKey(authKey, plaintext, true);
            Assert.Equal(messageKey, messageKeyActual.ToArray());
        }
        
        [Fact]
        public void ShouldGenerateClientMessageKey_From_Stream()
        {
            byte[] authKey = File.ReadAllBytes("testdata/crypto/authKeyClient");
            byte[] messageKey = File.ReadAllBytes("testdata/crypto/messageKeyClient");
            byte[] plaintext = File.ReadAllBytes("testdata/crypto/messageClientDecrypted");
            Stream plaintextStream = new MemoryStream(plaintext);
            var messageKeyActual = AesIge.GenerateMessageKey(authKey, plaintextStream, true);
            Assert.Equal(messageKey, messageKeyActual.ToArray());
        }

        [Fact]
        public void ShouldGenerateServerMessageKey()
        {
            byte[] authKey = File.ReadAllBytes("testdata/crypto/authKeyServer");
            byte[] messageKey = File.ReadAllBytes("testdata/crypto/messageKeyServer");
            byte[] plaintext = File.ReadAllBytes("testdata/crypto/messageServerDecrypted");
            Span<byte> actual = AesIge.GenerateMessageKey(authKey, plaintext);
            Assert.Equal(messageKey, actual.ToArray());
        }
        
        [Fact]
        public void ShouldGenerateServerMessageKey_From_Stream()
        {
            byte[] authKey = File.ReadAllBytes("testdata/crypto/authKeyServer");
            byte[] messageKey = File.ReadAllBytes("testdata/crypto/messageKeyServer");
            byte[] plaintext = File.ReadAllBytes("testdata/crypto/messageServerDecrypted");
            Stream plaintextStream = new MemoryStream(plaintext);
            Span<byte> actual = AesIge.GenerateMessageKey(authKey, plaintextStream);
            Assert.Equal(messageKey, actual.ToArray());
        }

        [Fact]
        public void ShouldDecryptMessageFromClient()
        {
            byte[] authKey = File.ReadAllBytes("testdata/crypto/authKeyClient");
            byte[] messageKey = File.ReadAllBytes("testdata/crypto/messageKeyClient");
            byte[] plaintext = File.ReadAllBytes("testdata/crypto/messageClientDecrypted");
            byte[] ciphertext = File.ReadAllBytes("testdata/crypto/messageClientEncrypted");
            var aes = new AesIge(authKey, messageKey);
            aes.Decrypt(ciphertext);
            Assert.Equal(plaintext, ciphertext);
        }

        [Fact]
        public void ShouldencryptMessageFromServer()
        {
            byte[] authKey = File.ReadAllBytes("testdata/crypto/authKeyServer");
            byte[] messageKey = File.ReadAllBytes("testdata/crypto/messageKeyServer");
            byte[] plaintext = File.ReadAllBytes("testdata/crypto/messageServerDecrypted");
            byte[] ciphertext = File.ReadAllBytes("testdata/crypto/messageServerEncrypted");
            Span<byte> actual = AesIge.GenerateMessageKey(authKey, plaintext);
            var aes = new AesIge(authKey, actual, false);
            aes.Encrypt(plaintext);
            Assert.Equal(ciphertext, plaintext);
        }
    }
}

