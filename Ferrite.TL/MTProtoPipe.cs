// 
// Project Ferrite is an Implementation of the Telegram Server API
// Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
// 

using System.Buffers;
using System.IO.Pipelines;
using System.Security.Cryptography;
using DotNext.Buffers;
using DotNext.IO;
using Ferrite.Crypto;

namespace Ferrite.TL;

public class MTProtoPipe : IDisposable
{
    private readonly Pipe _pipe;
    private readonly Aes _aes;
    private readonly byte[] _aesKey;
    private readonly byte[] _aesIV;
    private readonly IMemoryOwner<byte> _buff;
    private readonly bool _encrypt;
    public MTProtoPipe(Span<byte> aesKey, Span<byte> aesIV, bool encrypt)
    {
        _buff = UnmanagedMemoryAllocator.Allocate<byte>(16, false);
        _aesKey = new byte[32];
        _aesIV = new byte[32];
        aesKey.CopyTo(_aesKey);
        aesIV.CopyTo(_aesIV);
        _pipe = new Pipe();
        _aes = Aes.Create();
        _aes.Key = _aesKey;
        _encrypt = encrypt;
        
        Input = _pipe.Reader;
    }
    public void Write(ref SequenceReader reader)
    {
        while (reader.RemainingSequence.Length >= 16)
        {
            reader.Read(_buff.Memory.Span);
            if (_encrypt)
            {
                _aes.EncryptIge(_buff.Memory.Span, _aesIV);
            }
            else
            {
                _aes.DecryptIge(_buff.Memory.Span, _aesIV);
            }

            var buffer = _pipe.Writer.GetMemory(16);
            _buff.Memory.CopyTo(buffer);
            _pipe.Writer.Advance(16);
        }
        _pipe.Writer.FlushAsync();
    }
    public PipeReader Input { get; }

    public void Complete()
    {
        _pipe.Writer.Complete();
    }

    public void Dispose()
    {
        _buff.Dispose();
    }
}