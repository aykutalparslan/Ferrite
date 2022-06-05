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

namespace Ferrite.TL.slim;

public struct BoxedObject: ITLObjectReader, ITLSerializable
{
    public int Length { get; }
    public ReadOnlySpan<byte> ToReadOnlySpan()
    {
        throw new NotImplementedException();
    }

    public static ITLSerializable Read(Span<byte> data, in int offset, out int bytesRead)
    {
        throw new NotImplementedException();
    }

    public static unsafe ITLSerializable Read(byte* buffer, in int length, in int offset, out int bytesRead)
    {
        throw new NotImplementedException();
    }

    public static int ReadSize(Span<byte> data, in int offset)
    {
        throw new NotImplementedException();
    }

    public static unsafe int ReadSize(byte* buffer, in int length, in int offset)
    {
        throw new NotImplementedException();
    }

    public ref readonly int Constructor => throw new NotImplementedException();
}