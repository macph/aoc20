using System;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day11
{
    public class Day11 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day11.txt";

        public uint Day => 11;

        public string Title => "Seating System";

        public object SolvePart1()
            => ReadGrid().RunUntilStable(Mode.Adjacent).Occupied;

        public object SolvePart2()
            => ReadGrid().RunUntilStable(Mode.Visible).Occupied;

        private Grid ReadGrid()
        {
            using var stream = Utils.OpenResource(Resource);
            return Grid.ParseFromLines(Utils.ReadLines(stream));
        }

        private class Grid
        {
            private static readonly Direction[] Directions =
                (Direction[])Enum.GetValues(typeof(Direction));

            private readonly int height;
            private readonly int width;
            private readonly Seat[] grid;
            private readonly Seat[] previous;

            public int Height => height;

            public int Width => width;

            public int Occupied => grid.Where(s => s == Seat.Occupied).Count();

            private Grid(Seat[] grid, int height, int width)
            {
                if (height <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(height));
                }
                if (width <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(width));
                }
                if (grid.Length != height * width)
                {
                    throw new ArgumentException(
                        "Expected grid to have height * width items",
                        nameof(grid));
                }

                this.height = height;
                this.width = width;
                this.grid = grid;
                previous = new Seat[this.grid.Length];
            }

            public static Grid ParseFromLines(IEnumerable<string> input)
            {
                var height = 0;
                var width = 0;
                var list = new List<Seat>();

                foreach (string line in input)
                {
                    if (line.Length == 0)
                    {
                        throw new Exception("Expected non-empty line");
                    }
                    if (width != 0 && line.Length != width)
                    {
                        throw new Exception("Expected all lines to have the same length");
                    }
                    if (width == 0)
                    {
                        width = line.Length;
                    }
                    height++;

                    list.AddRange(
                        line.Select(c => c switch
                        {
                            '.' => Seat.Floor,
                            'L' => Seat.Empty,
                            '#' => Seat.Occupied,
                            _ => throw new Exception($"Expected '.', 'L' or '#', got '{c}'"),
                        }));
                }

                return new Grid(list.ToArray(), height, width);
            }

            private int AdjacentSeats(int index) =>
                Directions
                    .Select(d => Offset(index, d))
                    .Where(i => i >= 0 && previous[i] == Seat.Occupied)
                    .Count();

            private int VisibleSeats(int index) =>
                Directions.Where(d => VisibleInDirection(index, d)).Count();

            private bool VisibleInDirection(int index, Direction direction)
            {
                var d = 1;
                while (true)
                {
                    var i = Offset(index, direction, d);

                    if (i < 0 || previous[i] == Seat.Empty)
                    {
                        // No more seats in line of sight or first seat found is unoccupied.
                        return false;
                    }
                    if (previous[i] == Seat.Occupied)
                    {
                        // First seat found is occupied.
                        return true;
                    }
                    d++;
                }
            }

            private int Offset(int index, Direction direction, int distance = 1)
            {
                if (index < 0 || index >= grid.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                var row = index / width;
                var column = index % width;

                switch (direction)
                {
                    case Direction.North:
                        row -= distance;
                        break;
                    case Direction.NorthEast:
                        row -= distance;
                        column += distance;
                        break;
                    case Direction.East:
                        column += distance;
                        break;
                    case Direction.SouthEast:
                        row += distance;
                        column += distance;
                        break;
                    case Direction.South:
                        row += distance;
                        break;
                    case Direction.SouthWest:
                        row += distance;
                        column -= distance;
                        break;
                    case Direction.West:
                        column -= distance;
                        break;
                    case Direction.NorthWest:
                        row -= distance;
                        column -= distance;
                        break;
                    default:
                        throw new Exception("Expected a valid direction value");
                }

                // Return new index if both row and column are within limits.
                return (row >= 0 && row < height && column >= 0 && column < width)
                    ? row * width + column
                    : -1;
            }

            private bool Step(Mode mode)
            {
                // Set tolerance (maximum neighbouring seats before seat is vacated) and function
                // for determining number of neighbouring seats.
                int tolerance;
                Func<int, int> seatsOccupied;

                switch (mode)
                {
                    case Mode.Adjacent:
                        tolerance = 3;
                        seatsOccupied = AdjacentSeats;
                        break;
                    case Mode.Visible:
                        tolerance = 4;
                        seatsOccupied = VisibleSeats;
                        break;
                    default:
                        throw new Exception($"Expected valid mode value, got {mode}");
                }

                // Copy grid into previous and overwrite current.
                Array.Copy(grid, previous, grid.Length);

                var changed = false;

                for (var i = 0; i < previous.Length; i++)
                {
                    switch (previous[i])
                    {
                        case Seat.Empty when seatsOccupied(i) == 0:
                            // Occupy seat when no neighbouring seats are occupied.
                            grid[i] = Seat.Occupied;
                            changed = true;
                            break;
                        case Seat.Occupied when seatsOccupied(i) > tolerance:
                            // Vacate seat when occupied neighbouring seats exceeds tolerance.
                            grid[i] = Seat.Empty;
                            changed = true;
                            break;
                        case Seat.Floor:
                        case Seat.Empty:
                        case Seat.Occupied:
                            // Leave unchanged.
                            break;
                        default:
                            throw new Exception("Expected state to be floor, empty or occupied");
                    }
                }

                return changed;
            }

            public Grid RunUntilStable(Mode mode)
            {
                // Run until the grid is unchanged between steps.
                while (Step(mode)) ;
                return this;
            }
        }

        private enum Seat : byte
        {
            Floor,
            Empty,
            Occupied,
        }

        private enum Direction : byte
        {
            North,
            NorthEast,
            East,
            SouthEast,
            South,
            SouthWest,
            West,
            NorthWest,
        }

        private enum Mode : byte
        {
            Adjacent,
            Visible,
        }
    }
}
