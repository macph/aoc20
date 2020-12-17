using System;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day17
{
    class Day17 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day17.txt";

        public uint Day => 17;

        public string Title => "Conway Cubes";

        public object SolvePart1()
        {
            var grid = ReadGrid((x, y) => new ThreeVector(x, y, 0));
            grid.Execute(6);
            return grid.Count;
        }

        public object SolvePart2()
        {
            var grid = ReadGrid((x, y) => new FourVector(x, y, 0, 0));
            grid.Execute(6);
            return grid.Count;
        }

        private Grid<IVector> ReadGrid(Func<int, int, IVector> createOnPlane) =>
            new Grid<IVector>(FindCubes(createOnPlane));

        private IEnumerable<IVector> FindCubes(Func<int, int, IVector> createOnPlane)
        {
            using var stream = Utils.OpenResource(Resource);

            var width = -1;
            var y = 0;
            foreach (string line in Utils.ReadLines(stream))
            {
                if (width < 0)
                {
                    width = line.Length;
                }
                else if (line.Length != width)
                {
                    throw new Exception("Expected all lines to have same width");
                }

                var x = 0;
                foreach (char c in line)
                {
                    if (c == '#')
                    {
                        yield return createOnPlane(x, y);
                    }
                    else if (c != '.')
                    {
                        throw new Exception("Expected all characters to be '#' or '.'");
                    }
                    x++;
                }
                y--;
            }
        }

        private class Grid<V> where V : notnull, IVector
        {
            private HashSet<V> grid;
            private HashSet<V> grid0;
            private Dictionary<V, byte> adjacent;
            private Dictionary<V, byte> adjacent0;
            private int step;

            public IEnumerable<V> Cubes => grid;

            public int Count => grid.Count;

            public int Step => step;

            public Grid(IEnumerable<V> cubes)
            {
                grid = new HashSet<V>(cubes);
                grid0 = new HashSet<V>();
                adjacent = new Dictionary<V, byte>();
                adjacent0 = new Dictionary<V, byte>();
                step = 0;

                // Set up initial adjacent grid.
                foreach (V v in grid)
                {
                    foreach (V adj in v.Adjacent())
                    {
                        Increment(adjacent, adj);
                    }
                }
            }

            public void Execute(int cycles)
            {
                for (var i = 0; i < cycles; i++)
                {
                    Execute();
                }
            }

            public void Execute()
            {
                // Swap current and previous grids to save on allocation.
                Swap(ref grid, ref grid0);
                Swap(ref adjacent, ref adjacent0);
                adjacent.Clear();
                grid.Clear();

                foreach (KeyValuePair<V, byte> pair in adjacent0)
                {
                    var vector = pair.Key;
                    var isActive = grid0.Contains(vector);
                    var countAdjacent = pair.Value;
                    if (isActive && countAdjacent == 2 ||
                        isActive && countAdjacent == 3 ||
                        !isActive && countAdjacent == 3)
                    {
                        grid.Add(vector);
                        foreach (V adj in vector.Adjacent())
                        {
                            Increment(adjacent, adj);
                        }
                    }
                }

                step++;
            }

            private static void Swap<T>(ref T left, ref T right)
            {
                var temp = left;
                left = right;
                right = temp;
            }

            private static void Increment(Dictionary<V, byte> dictionary, V vector)
            {
                if (!dictionary.ContainsKey(vector))
                {
                    dictionary[vector] = 0;
                }
                dictionary[vector] += 1;
            }
        }

        private interface IVector
        {
            public IEnumerable<IVector> Adjacent();
        }

        private struct ThreeVector : IVector
        {
            private static readonly ThreeVector[] Offsets = FindOffset().ToArray();

            private readonly int x;
            private readonly int y;
            private readonly int z;

            public ThreeVector(int x, int y, int z)
            {
                this.x = x;
                this.y = y;
                this.z = z;
            }

            public IEnumerable<IVector> Adjacent()
            {
                var vector = this;
                return Offsets.Select(offset => (IVector)(vector + offset));
            }

            private static IEnumerable<ThreeVector> FindOffset()
            {
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        for (int k = -1; k < 2; k++)
                        {
                            var v = new ThreeVector(i, j, k);
                            if (v != default)
                            {
                                yield return v;
                            }
                        }
                    }
                }
            }

            public override bool Equals(object? obj) => obj is ThreeVector vector && Equals(vector);

            public bool Equals(ThreeVector other) => x == other.x && y == other.y && z == other.z;

            public override int GetHashCode() => (x, y, z).GetHashCode();

            public static bool operator ==(ThreeVector left, ThreeVector right) => left.Equals(right);

            public static bool operator !=(ThreeVector left, ThreeVector right) => !left.Equals(right);

            public static ThreeVector operator +(ThreeVector left, ThreeVector right) =>
                new ThreeVector(left.x + right.x, left.y + right.y, left.z + right.z);

            public static ThreeVector operator -(ThreeVector left, ThreeVector right) =>
                new ThreeVector(left.x - right.x, left.y - right.y, left.z - right.z);
        }

        private struct FourVector : IVector, IEquatable<FourVector>
        {
            private static readonly FourVector[] Offsets = FindOffset().ToArray();

            private readonly int x;
            private readonly int y;
            private readonly int z;
            private readonly int w;

            public FourVector(int x, int y, int z, int w)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.w = w;
            }

            public IEnumerable<IVector> Adjacent()
            {
                var vector = this;
                return Offsets.Select(offset => (IVector)(vector + offset));
            }

            private static IEnumerable<FourVector> FindOffset()
            {
                for (int i = -1; i < 2; i++)
                {
                    for (int j = -1; j < 2; j++)
                    {
                        for (int k = -1; k < 2; k++)
                        {
                            for (int l = -1; l < 2; l++)
                            {
                                var v = new FourVector(i, j, k, l);
                                if (v != default)
                                {
                                    yield return v;
                                }
                            }
                        }
                    }
                }
            }

            public override bool Equals(object? obj) => obj is FourVector vector && Equals(vector);

            public bool Equals(FourVector other) =>
                x == other.x && y == other.y && z == other.z && w == other.w;

            public override int GetHashCode() => (x, y, z, w).GetHashCode();

            public static bool operator ==(FourVector left, FourVector right) => left.Equals(right);

            public static bool operator !=(FourVector left, FourVector right) => !left.Equals(right);

            public static FourVector operator +(FourVector left, FourVector right) =>
                new FourVector(left.x + right.x, left.y + right.y, left.z + right.z, left.w + right.w);

            public static FourVector operator -(FourVector left, FourVector right) =>
                new FourVector(left.x - right.x, left.y - right.y, left.z - right.z, left.w - right.w);
        }
    }
}
