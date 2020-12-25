using System;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day24
{
    public class Day24 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day24.txt";

        public uint Day => 24;

        public string Title => "Lobby Layout";

        public object SolvePart1() => new HexagonalGrid(ReadDirections()).Count;

        public object SolvePart2() => new HexagonalGrid(ReadDirections()).Update(100).Count;

        private IEnumerable<Vector> ReadDirections()
        {
            using var stream = Utils.OpenResource(Resource);
            foreach (string line in Utils.ReadLines(stream))
            {
                // Combine instructions from each line into a single vector.
                yield return ParseDirections(line)
                    .Select(d => Directions.Vectors[d])
                    .Aggregate((a, b) => a + b);
            }
        }

        private IEnumerable<Direction> ParseDirections(string input)
        {
            for (var i = 0; i < input.Length; i++)
            {
                var next = (i + 1 < input.Length) ? input[i + 1] : default;
                switch (input[i])
                {
                    case 'e':
                        yield return Direction.East;
                        break;
                    case 's' when next == 'e':
                        yield return Direction.SouthEast;
                        i++;
                        break;
                    case 's' when next == 'w':
                        yield return Direction.SouthWest;
                        i++;
                        break;
                    case 'w':
                        yield return Direction.West;
                        break;
                    case 'n' when next == 'w':
                        yield return Direction.NorthWest;
                        i++;
                        break;
                    case 'n' when next == 'e':
                        yield return Direction.NorthEast;
                        i++;
                        break;
                    default:
                        throw new Exception(
                            "Expected 'e', 'se', 'sw', 'w', 'nw' or 'ne' in sequence");
                }
            }
        }
    }

    public class HexagonalGrid
    {
        private HashSet<Vector> grid;
        private Dictionary<Vector, int> adjacent;

        public int Count => grid.Count;

        public HexagonalGrid(IEnumerable<Vector> positions)
        {
            grid = new HashSet<Vector>();
            adjacent = new Dictionary<Vector, int>();

            foreach (Vector position in positions)
            {
                Flip(position);
            }
        }

        public void Flip(Vector position)
        {
            if (grid.Add(position))
            {
                // Tile set; increment all adjacent tiles by 1.
                UpdateAdjacent(position, 1);
            }
            else
            {
                // Tile was set and is unset; decrement all adjacent tiles by 1.
                grid.Remove(position);
                UpdateAdjacent(position, -1);
            }
        }

        private void UpdateAdjacent(Vector position, int delta)
        {
            foreach (Vector unit in Directions.Vectors.Values)
            {
                var offset = position + unit;
                adjacent[offset] = adjacent.GetValueOrDefault(offset) + delta;
            }
        }

        public HexagonalGrid Update(int days = 1)
        {
            for (var i = 0; i < days; i++)
            {
                Update();
            }

            return this;
        }

        private void Update()
        {
            var oldGrid = grid;
            var oldAdjacent = adjacent;

            grid = new HashSet<Vector>();
            adjacent = new Dictionary<Vector, int>();

            foreach (Vector position in oldGrid)
            {
                // For all tiles already set, check if adjacent tiles equal 1 or 2.
                var adj = oldAdjacent.GetValueOrDefault(position);
                if (adj == 1 || adj == 2)
                {
                    Flip(position);
                }
            }
            foreach (Vector position in oldAdjacent.Keys.Except(oldGrid))
            {
                // For all tiles not already set, check if adjacent tiles equal 2.
                if (oldAdjacent.GetValueOrDefault(position) == 2)
                {
                    Flip(position);
                }
            }
        }
    }

    public struct Vector : IEquatable<Vector>
    {
        public int X { get; }

        public int Y { get; }

        public Vector(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override int GetHashCode() => (X, Y).GetHashCode();

        public override bool Equals(object? obj) => obj is Vector vector && Equals(vector);

        public bool Equals(Vector other) => X == other.X && Y == other.Y;

        public static bool operator ==(Vector left, Vector right) => left.Equals(right);

        public static bool operator !=(Vector left, Vector right) => !left.Equals(right);

        public static Vector operator +(Vector vector) => vector;

        public static Vector operator -(Vector vector) => new Vector(-vector.X, -vector.Y);

        public static Vector operator +(Vector left, Vector right) =>
            new Vector(left.X + right.X, left.Y + right.Y);

        public static Vector operator -(Vector left, Vector right) =>
            new Vector(left.X - right.X, left.Y - right.Y);
    }

    public enum Direction
    {
        East,
        SouthEast,
        SouthWest,
        West,
        NorthWest,
        NorthEast,
    }

    public static class Directions
    {
        // Let X axis be west-east and Y axis be southwest-northeast, then northwest-southeast is
        // the diagonal with slope -1 on the grid.
        public static readonly IReadOnlyDictionary<Direction, Vector> Vectors =
            new Dictionary<Direction, Vector>
        {
            { Direction.East, new Vector(1, 0) },
            { Direction.SouthEast, new Vector(1, -1) },
            { Direction.SouthWest, new Vector(0, -1) },
            { Direction.West, new Vector(-1, 0) },
            { Direction.NorthWest, new Vector(-1, 1) },
            { Direction.NorthEast, new Vector(0, 1) },
        };
    }
}
