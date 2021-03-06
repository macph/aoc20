﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day01
{
    public class Day01 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day01.txt";

        public uint Day => 1;

        public string Title => "Report Repair";

        public object SolvePart1() => FindSumEntriesAndMultiply(2020, 2);

        public object SolvePart2() => FindSumEntriesAndMultiply(2020, 3);

        private int FindSumEntriesAndMultiply(int sum, int entries)
        {
            var numbers = ReadNumbersFromFile();
            var found = FindSumEntries(numbers, sum, entries) ??
                throw new Exception($"No set of values summing to 2020 was found");

            return found.Aggregate(1, (a, b) => a * b);
        }

        private List<int>? FindSumEntries(List<int> numbers, int required, int remainingEntries)
        {
            return FindSumEntries(
                new ReadOnlySpan<int>(numbers.ToArray()),
                required,
                remainingEntries);
        }

        private List<int>? FindSumEntries(
            ReadOnlySpan<int> numbers,
            int required,
            int remainingEntries)
        {
            for (int i = 0; i < numbers.Length; i++)
            {
                var remainder = required - numbers[i];

                if (remainder == 0 && remainingEntries == 1)
                {
                    // Last entry adds to required sum; return to end recursion.
                    return new List<int> { numbers[i] };
                }
                else if (remainder > 0 && remainingEntries > 1)
                {
                    // Sum after adding entry still short of sum - continue looking for extra
                    // entries.
                    var result = FindSumEntries(
                        numbers[(i + 1)..],
                        remainder,
                        remainingEntries - 1);

                    if (result != null)
                    {
                        // Correct combination of numbers found, return with this entry.
                        result.Add(numbers[i]);
                        return result;
                    }
                }
            }

            return null;
        }

        private List<int> ReadNumbersFromFile()
        {
            using var stream = Utils.OpenResource(Resource);
            return Utils.ReadLines(stream).Select(int.Parse).ToList();
        }
    }
}
