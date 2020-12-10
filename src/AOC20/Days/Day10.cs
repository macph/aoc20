using System;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day10
{
    public class Day10 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day10.txt";

        public uint Day => 10;

        public object SolvePart1()
        {
            var differences = new Adapters(ReadNumbers()).Differences();
            return differences[1] * differences[3];
        }

        public object SolvePart2() => new Adapters(ReadNumbers()).Count();

        private IEnumerable<int> ReadNumbers()
        {
            using var stream = Utils.OpenResource(Resource);
            foreach (string line in Utils.ReadLines(stream))
            {
                yield return int.Parse(line);
            }
        }

        private class Adapters
        {
            private readonly int[] adapters;
            private readonly Dictionary<int, long> memoized;

            public Adapters(IEnumerable<int> adapters)
            {
                this.adapters = adapters.ToArray();
                Array.Sort(this.adapters);

                memoized = new Dictionary<int, long>();
            }

            public int[] Differences()
            {
                // With list sorted every pair of adjacent adapters should be separated by 1-3 
                // jolts.
                var differences = new int[4];

                // Initial joltage at outlet is 0, add difference with first adapter to list.
                differences[adapters[0]] += 1;
                // Final joltage at device is 3 higher than last adapter, add that.
                differences[3] += 1;

                for (var i = 1; i < adapters.Length; i++)
                {
                    var value = adapters[i] - adapters[i - 1];
                    differences[value] += 1;
                }

                return differences;
            }

            public long Count() => Count(0);

            private long Count(int initial)
            {
                if (memoized.TryGetValue(initial, out var cached))
                {
                    // Result already exists in cache.
                    return cached;
                }

                // List of adapters should already be sorted.
                var found = Array.BinarySearch(adapters, initial);
                var start = (found >= 0) ? found + 1 : ~found;

                if (start == adapters.Length)
                {
                    // This is the last adapter in list and the device is the only one left.
                    return memoized[initial] = 1L;
                }

                var total = 0L;
                for (var i = start; i < adapters.Length && adapters[i] <= initial + 3; i++)
                {
                    total += Count(adapters[i]);
                }

                // Add result to cache too.
                return memoized[initial] = total;
            }
        }
    }
}
