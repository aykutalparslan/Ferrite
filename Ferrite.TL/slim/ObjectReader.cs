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

using System.Runtime.InteropServices;

namespace Ferrite.TL.slim;

public static class ObjectReader2
{
    private static readonly Dictionary<int, ObjectReaderDelegate> _objectReaders = new();
    private static readonly Dictionary<int, ObjectSizeReaderDelegate> _sizeReaders = new();

    static ObjectReader2()
    {
        _objectReaders.Add(unchecked((int)0xc4b9f9bb), error.Read);
        _sizeReaders.Add(unchecked((int)0xc4b9f9bb), error.ReadSize);
    }

    public static Span<byte> Read(Span<byte> buff)
    {
        if (buff.Length < 4)
        {
            return Span<byte>.Empty;
        }
        int constructor = MemoryMarshal.Read<int>(buff);
        if (_objectReaders.ContainsKey(constructor))
        {
            var reader = _objectReaders[constructor];
            return reader.Invoke(buff, 0);
        }
        return Span<byte>.Empty;
    }
    public static Span<byte> Read(Span<byte> buff, int constructor)
    {
        if (buff.Length < 4)
        {
            return Span<byte>.Empty;
        }
        if (_objectReaders.ContainsKey(constructor))
        {
            var reader = _objectReaders[constructor];
            return reader.Invoke(buff, 0);
        }
        return Span<byte>.Empty;
    }
    public static int ReadSize(Span<byte> buff)
    {
        if (buff.Length < 4)
        {
            return 0;
        }
        int constructor = MemoryMarshal.Read<int>(buff);
        if (_sizeReaders.ContainsKey(constructor))
        {
            var reader = _sizeReaders[constructor];
            return reader.Invoke(buff, 0);
        }
        return 0;
    }
    public static int ReadSize(Span<byte> buff, int constructor)
    {
        if (buff.Length < 4)
        {
            return 0;
        }
        if (_sizeReaders.ContainsKey(constructor))
        {
            var reader = _sizeReaders[constructor];
            return reader.Invoke(buff, 0);
        }
        return 0;
    }
    public static ObjectReaderDelegate? GetObjectReader(int constructor)
    {
        if (_objectReaders.ContainsKey(constructor))
        {
            return _objectReaders[constructor];
        }

        return null;
    }
    public static ObjectSizeReaderDelegate? GetObjectSizeReader(int constructor)
    {
        if (_sizeReaders.ContainsKey(constructor))
        {
            return _sizeReaders[constructor];
        }

        return null;
    }
}