//
//    Project Ferrite is an Implementation Telegram Server API
//    Copyright 2022 Aykut Alparslan KOC <aykutalparslan@msn.com>
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU Affero General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU Affero General Public License for more details.
//
//    You should have received a copy of the GNU Affero General Public License
//    along with this program.  If not, see <https://www.gnu.org/licenses/>.
//
using System;
using System.Collections;

namespace Ferrite.Utils;

public class CircularQueue<T> : IEnumerable<T> where T : unmanaged
{
    private T[] _array;
    private int _head = 0;
    private int _tail = 0;
    private int _count = 0;
    public int Count => _count;
    public CircularQueue(int limit)
    {
        _array = new T[limit];
    }
    public void Enqueue(T item)
    {
        if (_array.Length == _count)
        {
            _head = (_head + 1) % _array.Length;
        }
        _array[_tail] = item;
        _tail = (_tail + 1) % _array.Length;
        _count++;
        if (_count > _array.Length)
        {
            _count = _array.Length;
        }
    }
    public T Peek()
    {
        return _array[_head];
    }
    public T Dequeue()
    {
        T t = _array[_head];
        _head = (_head + 1) % _array.Length;
        _count--;
        return t;
    }

    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < _count; i++)
        {
            yield return _array[(_head + i) % _array.Length];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}