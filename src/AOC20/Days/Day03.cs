using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AOC20.Day03
{
    public class Day03 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day03.txt";

        public uint Day => 3;

        public string Title => "Toboggan Trajectory";

        public object SolvePart1()
        {
            var forest = ReadForestFromFile();
            return forest.CountTreesOnSlope(1, 3);
        }

        public object SolvePart2()
        {
            var forest = ReadForestFromFile();
            var slopes = new (int, int)[]
            {
                (1, 1),
                (1, 3),
                (1, 5),
                (1, 7),
                (2, 1),
            };

            // Expected product of trees will overflow for int32, so cast to long first.
            return slopes
                .Select(pair => (long)forest.CountTreesOnSlope(pair.Item1, pair.Item2))
                .Aggregate(1L, (a, b) => a * b);
        }

        private Forest ReadForestFromFile()
        {
            using var stream = Utils.OpenResource(Resource);
            return new Forest(Utils.ReadLines(stream));
        }

        private class Forest
        {
            private readonly int rows;
            private readonly int columns;
            private readonly bool[] trees;

            public int Rows
            {
                get => rows;
            }

            public bool this[int row, int column]
            {
                get => IsTree(row, column);
            }

            public Forest(IEnumerable<string> lines)
            {
                int count = 0;
                int length = -1;
                var treesRead = new List<bool>();

                foreach (string line in lines)
                {
                    if (length < 0)
                    {
                        length = line.Length;
                    }
                    else if (line.Length != length)
                    {
                        throw new ArgumentException(
                            "Expected all lines to have equal widths.",
                            nameof(lines));
                    }
                    treesRead.AddRange(line.Select(ReadChar));
                    count++;
                }

                rows = count;
                columns = length;
                trees = treesRead.ToArray();

                Debug.Assert(trees.Length == rows * columns);
            }

            private bool ReadChar(char c) =>
                c switch
                {
                    '.' => false,
                    '#' => true,
                    _ => throw new Exception($"Expected '.' or '#', got {c}")
                };

            public bool IsTree(int row, int column)
            {
                if (row < 0 || row >= rows) throw new ArgumentOutOfRangeException(nameof(row));
                if (column < 0) throw new ArgumentOutOfRangeException(nameof(column));

                return trees[row * columns + column % columns];
            }

            public int CountTreesOnSlope(int down, int right) =>
                FollowSlope(down, right).Where(t => t).Count();

            public IEnumerable<bool> FollowSlope(int down, int right) =>
                Enumerable.Range(0, rows / down).Select(i => IsTree(i * down, i * right));
        }
    }
}
