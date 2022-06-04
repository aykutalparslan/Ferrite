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

namespace Ferrite.TL.slim.mtproto;

public readonly unsafe struct ResPq : ITLStruct<ResPq>
{
    private readonly byte* _buff;
    public ref readonly int Constructor => throw new NotImplementedException();

    public int Length { get; }
    public ReadOnlySpan<byte> ToReadOnlySpan() => new ReadOnlySpan<byte>(_buff, Length);
    public static ResPq Read(Span<byte> data, in int offset, out int bytesRead)
    {
        throw new NotImplementedException();
    }

    public static ResPq Read(byte* buffer, in int length, in int offset, out int bytesRead)
    {
        throw new NotImplementedException();
    }

    public static int ReadSize(Span<byte> data, in int offset)
    {
        throw new NotImplementedException();
    }

    public static int ReadSize(byte* buffer, in int length, in int offset)
    {
        throw new NotImplementedException();
    }

    public static ResPq Init(Span<byte> data, in int offset, in int length)
    {
        throw new NotImplementedException();
    }

    public static ResPq Read(Span<byte> data, int offset, out int bytesRead)
    {
        throw new NotImplementedException();
    }

    public static int ReadSize(Span<byte> data, int offset)
    {
        throw new NotImplementedException();
    }
}