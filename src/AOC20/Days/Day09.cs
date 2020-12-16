using System;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day9
{
    public class Day9 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day09.txt";

        public uint Day => 9;

        public string Title => "Encoding Error";

        public object SolvePart1() => GetFirstInvalidNumber(ReadNumbers());

        public object SolvePart2()
        {
            var list = ReadNumbers().ToList();
            var invalid = GetFirstInvalidNumber(list);

            int i;
            int j = 0;
            bool found = false;

            for (i = 0; i < list.Count; i++)
            {
                var sum = 0L;
                for (j = i; j < list.Count; j++)
                {
                    sum += list[j];
                    if (sum >= invalid)
                    {
                        break;
                    }
                }
                if (sum == invalid)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw new Exception(
                    $"Cannot find a continguous sequence of numbers summing {invalid}");
            }

            var sequence = list.Skip(i).Take(j + 1 - i);
            return sequence.Min() + sequence.Max();
        }

        private long GetFirstInvalidNumber(IEnumerable<long> numbers)
        {
            const int size = 25;

            var queue = new Queue<long>(numbers.Take(size));
            foreach (long number in numbers.Skip(size))
            {
                if (!FindSum(queue, number))
                {
                    // Return first number not equal to any sum of pairs in the preceding 25
                    // numbers.
                    return number;
                }
                queue.Dequeue();
                queue.Enqueue(number);
            }

            return -1;
        }

        private bool FindSum(IEnumerable<long> numbers, long sum) =>
            // Find pair of numbers in enumerable summing to get the required number.
            numbers.Where((a, i) => numbers.Skip(i + 1).Any(b => a + b == sum)).Any();

        private IEnumerable<long> ReadNumbers()
        {
            // Integers in list exceed the maximum int value, use long instead.
            using var stream = Utils.OpenResource(Resource);
            foreach (string line in Utils.ReadLines(stream))
            {
                yield return long.Parse(line);
            }
        }
    }
}
