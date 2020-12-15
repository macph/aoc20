using System;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day15
{
    class Day15 : ISolvable
    {
        private readonly int[] Initial = new int[] { 0, 6, 1, 7, 2, 19, 20 };

        public uint Day => 15;

        public object SolvePart1() =>
            ReciteNumbers(Initial).Skip(2020 - 1).First();

        public object SolvePart2() =>
            ReciteNumbers(Initial).Skip(30000000 - 1).First();

        private IEnumerable<int> ReciteNumbers(IEnumerable<int> initial)
        {
            // TODO: Is this a sequence with a recurrence relation or function?

            var spoken = new List<int>();
            var last = -1;
            var turn = 0;

            foreach (int number in initial)
            {
                if (number < 0)
                {
                    throw new ArgumentException(
                        "All initial numbers must be positive", nameof(initial));
                }
                if (last >= 0)
                {
                    ExpandWith(spoken, -1, last, turn);
                }
                // Yield each number from initial list in turn.
                yield return number;

                last = number;
                turn++;
            }

            int current;
            while (true)
            {
                // Yield 0 if the last number was not used previously, otherwise get the number of
                // turns since the last time it was used.
                current = last < spoken.Count && spoken[last] >= 0 ? turn - spoken[last] : 0;
                yield return current;

                ExpandWith(spoken, -1, last, turn);
                last = current;
                turn++;
            }
        }

        private void ExpandWith<T>(IList<T> list, T defaultValue, int index, T value)
        {
            while (index >= list.Count)
            {
                list.Add(defaultValue);
            }
            list[index] = value;
        }
    }
}
