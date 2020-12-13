using System;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day12
{
    public class Day12 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day12.txt";

        public uint Day => 12;

        public object SolvePart1() =>
            // Start at origin, facing east with waypoint 1 east.
            ReadInstructions()
                .Aggregate(new Ship(default, 1, 0), (s, i) => s.Move(i, false))
                .Position
                .Manhattan();

        public object SolvePart2() =>
            // Start at origin, with waypoint 10 east 1 north.
            ReadInstructions()
                .Aggregate(new Ship(default, 10, 1), (s, i) => s.Move(i, true))
                .Position
                .Manhattan();

        private IEnumerable<Instruction> ReadInstructions()
        {
            using var stream = Utils.OpenResource(Resource);
            foreach (string line in Utils.ReadLines(stream))
            {
                yield return Instruction.Parse(line);
            }
        }

        private struct Ship
        {
            public Vector Position { get; }

            public Vector Waypoint { get; }

            public Ship(Vector position, Vector waypoint)
            {
                Position = position;
                Waypoint = waypoint;
            }

            public Ship(Vector position, int waypointX, int waypointY)
            {
                Position = position;
                Waypoint = new Vector(waypointX, waypointY);
            }

            public Ship(int positionX, int positionY, Vector waypoint)
            {
                Position = new Vector(positionX, positionY);
                Waypoint = waypoint;
            }

            public Ship(int positionX, int positionY, int waypointX, int waypointY)
            {
                Position = new Vector(positionX, positionY);
                Waypoint = new Vector(waypointX, waypointY);
            }

            public Ship Move(Instruction instruction, bool withWaypoint)
            {
                switch (instruction.Action)
                {
                    // Move waypoint instead of position when withWaypoint is enabled.
                    case Action.North when withWaypoint:
                        return new Ship(Position, Waypoint.X, Waypoint.Y + instruction.Value);
                    case Action.East when withWaypoint:
                        return new Ship(Position, Waypoint.X + instruction.Value, Waypoint.Y);
                    case Action.South when withWaypoint:
                        return new Ship(Position, Waypoint.X, Waypoint.Y - instruction.Value);
                    case Action.West when withWaypoint:
                        return new Ship(Position, Waypoint.X - instruction.Value, Waypoint.Y);

                    case Action.North:
                        return new Ship(Position.X, Position.Y + instruction.Value, Waypoint);
                    case Action.East:
                        return new Ship(Position.X + instruction.Value, Position.Y, Waypoint);
                    case Action.South:
                        return new Ship(Position.X, Position.Y - instruction.Value, Waypoint);
                    case Action.West:
                        return new Ship(Position.X - instruction.Value, Position.Y, Waypoint);

                    case Action.Left:
                    case Action.Right:
                        // Set counter-clockwise degrees for rotating waypoint vector.
                        var degrees = (instruction.Action == Action.Left)
                            ? instruction.Value
                            : 360 - instruction.Value;
                        return new Ship(Position, Waypoint.Rotate(degrees));

                    case Action.Forward:
                        return new Ship(
                            Position.X + Waypoint.X * instruction.Value,
                            Position.Y + Waypoint.Y * instruction.Value,
                            Waypoint);

                    default:
                        throw new Exception(
                            $"Expected valid Action value, got {instruction.Action}");
                }
            }
        }

        private struct Vector
        {
            public int X { get; }

            public int Y { get; }

            public Vector(int x, int y)
            {
                X = x;
                Y = y;
            }

            public Vector Rotate(int degrees)
            {
                if (degrees % 90 != 0)
                {
                    throw new ArgumentException("Expected a multiple of 90", nameof(degrees));
                }

                var radians = 2.0 * Math.PI / 360.0 * degrees;
                // Use 2D rotation matrix to rotate vector around origin counter-clockwise.
                // Both cosine and sine values are expected to be integers -1, 0 or 1.
                var cosine = Convert.ToInt32(Math.Cos(radians));
                var sine = Convert.ToInt32(Math.Sin(radians));
                return new Vector(cosine * X - sine * Y, sine * X + cosine * Y);
            }

            public int Manhattan(Vector other = default) =>
                Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
        }

        private struct Instruction
        {
            public Action Action { get; }
            
            public int Value { get; }

            public Instruction(Action action, int value)
            {
                Action = action;
                Value = value;
            }

            public static Instruction Parse(string input)
            {
                if (input.Length < 2)
                {
                    throw new FormatException("At least two characters expected");
                }

                var action = input[0] switch
                {
                    'N' => Action.North,
                    'E' => Action.East,
                    'S' => Action.South,
                    'W' => Action.West,
                    'L' => Action.Left,
                    'R' => Action.Right,
                    'F' => Action.Forward,
                    _ => throw new FormatException(
                        $"Expected valid first character, got {input[0]}"),
                };
                var value = int.Parse(input[1..]);

                return new Instruction(action, value);
            }
        }

        private enum Action
        {
            North,
            East,
            South,
            West,
            Left,
            Right,
            Forward,
        }
    }
}
