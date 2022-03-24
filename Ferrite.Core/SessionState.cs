/*
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

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Ferrite.Core;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct SessionState
{
    public SessionState(ref byte source)
    {
        Unsafe.SkipInit(out this);
        this = Unsafe.As<byte, SessionState>(ref source);
    }
    [FieldOffset(0)]
    public long SessionId;
    [FieldOffset(8)]
    public long AuthKeyId;
    [FieldOffset(16)]
    private fixed byte _authKey[128];
    public ReadOnlySpan<byte> AuthKey
    {
        get
        {
            fixed (byte* p = _authKey)
            {
                return new ReadOnlySpan<byte>(p, 128);
            }
        }
        set
        {
            if (value.Length == 128)
            {
                fixed (byte* p = _authKey)
                fixed (byte* v = &MemoryMarshal.GetReference(value))
                {
                    Buffer.MemoryCopy(v, p, 128, 128);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }
    }
    [FieldOffset(144), MarshalAs(UnmanagedType.Struct, SizeConst = 16)]
    public Guid NodeId;
    [FieldOffset(160)]
    public long ServerSalt;
    [FieldOffset(168)]
    public long ServerSaltOld;
    [FieldOffset(0)]
    private fixed byte _bytes[176];
    public ReadOnlySpan<byte> Span
    {
        get
        {
            fixed (byte* p = _bytes)
            {
                return new ReadOnlySpan<byte>(p, sizeof(SessionState));
            }
        }
    }
    public byte[] ToByteArray()
    {
        byte[] buff = new byte[sizeof(SessionState)];
        fixed (byte* p = _bytes)
        {
            Marshal.Copy((IntPtr)p, buff, 0, sizeof(SessionState));
        }
        return buff;
    }
    public static SessionState Read(ReadOnlySpan<byte> from)
    {
        return MemoryMarshal.Cast<byte, SessionState>(from)[0];
    }
}