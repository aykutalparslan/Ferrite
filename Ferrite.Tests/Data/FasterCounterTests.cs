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

using System.Collections.Concurrent;
using Ferrite.Data;
using Xunit;

namespace Ferrite.Tests.Data;

public class FasterCounterTests
{
    [Fact]
    public async void FasterCounter_Should_IncrementAndGet()
    {
        FasterContext<string, long> context = new FasterContext<string, long>();
        FasterCounter counter1 = new FasterCounter(context, "counter1");
        FasterCounter counter2 = new FasterCounter(context, "counter2");
        ConcurrentBag<long> bag = new();
        ConcurrentBag<long> bag2 = new();
        Thread[] threads = new Thread[10];
        for (int i = 0; i < threads.Length; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    bag.Add(counter1.IncrementAndGet().GetAwaiter().GetResult());
                }
            });
        }
        Thread[] threads2 = new Thread[10];
        for (int i = 0; i < threads.Length; i++)
        {
            threads2[i] = new Thread(() =>
            {
                for (int i = 0; i < 100; i++)
                {
                    bag2.Add(counter2.IncrementAndGet().GetAwaiter().GetResult());
                }
            });
        }
        foreach(Thread thread in threads)
        {
            thread.Start();
        }
        foreach(Thread thread in threads2)
        {
            thread.Start();
        }
        foreach(Thread thread in threads)
        {
            thread.Join();
        }
        foreach(Thread thread in threads2)
        {
            thread.Join();
        }

        for (long i = 1; i <= 1000; i++)
        {
            Assert.True(bag.Contains(i));
        }
        for (long i = 1; i <= 1000; i++)
        {
            Assert.True(bag2.Contains(i));
        }
    }
}