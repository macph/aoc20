using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOC20.Day20
{
    class Day20 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day20.txt";

        public uint Day => 20;

        public string Title => "Jurassic Jigsaw";

        public object SolvePart1() =>
            ReadTiles().ArrangeTiles().CornerIds().Aggregate(1L, (a, b) => a * b);

        public object SolvePart2()
        {
            throw new NotImplementedException();
        }

        private TileSet ReadTiles()
        {
            using var stream = Utils.OpenResource(Resource);
            using var lines = Utils.ReadLines(stream, skipEmpty: false).GetEnumerator();

            var set = new TileSet();

            while (lines.MoveNext())
            {
                if (!lines.Current.StartsWith("Tile ") || !lines.Current.EndsWith(":"))
                {
                    throw new Exception("Expected a line of form 'Tile 0123:'");
                }
                var id = int.Parse(lines.Current[5..^1]);

                var pixels = new List<bool>();
                while (lines.MoveNext() && !string.IsNullOrWhiteSpace(lines.Current))
                {
                    pixels.AddRange(lines.Current.Select(ParsePixel));
                }

                set.Add(id, new Tile(pixels));
            }

            return set;
        }

        private bool ParsePixel(char c) => c switch
        {
            '.' => false,
            '#' => true,
            _ => throw new ArgumentException("Expected '.' or '#'", nameof(c)),
        };
    }

    public class TileSet
    {
        private readonly Dictionary<int, Tile> tiles;
        private readonly Dictionary<int, List<MatchingTile>> cache;

        public TileSet()
        {
            tiles = new Dictionary<int, Tile>();
            cache = new Dictionary<int, List<MatchingTile>>();
        }

        public void Add(int id, Tile tile)
        {
            tiles.Add(id, tile);
            cache.Clear();
        }

        public ArrangedTiles ArrangeTiles()
        {
            var found = new ArrangedTiles();

            var first = tiles.Keys.First();
            found.Add(first, tiles[first], default, Transformation.None);

            return FindNextTile(found, first).First();
        }

        private IEnumerable<ArrangedTiles> FindNextTile(ArrangedTiles found, int next)
        {
            if (found.Count == tiles.Count)
            {
                yield return found;
                yield break;
            }

            List<MatchingTile> matches;

            if (cache.ContainsKey(next))
            {
                matches = cache[next];
            }
            else
            {
                matches = tiles.Keys
                    .Where(other => other != next)
                    .SelectMany(other =>
                        tiles[next]
                            .Matches(tiles[other])
                            .Select(p => new MatchingTile(other, p.Edge, p.Tr)))
                    .ToList();
                cache[next] = matches;
            }

            var position = found.GetPosition(next);
            var tr = found.GetTransformation(next);

            foreach (MatchingTile match in matches)
            {
                var tileId = match.Id;
                var tile = tiles[tileId].Transform(tr.Combine(match.Tr));
                var tilePosition = position + match.Edge.Offset();
                var tileTr = match.Tr;

                if (!found.IsValid(tileId, tile, tilePosition))
                {
                    continue;
                }

                var newFound = new ArrangedTiles(found);
                newFound.Add(tileId, tile, tilePosition, tileTr);

                foreach (ArrangedTiles nextFound in FindNextTile(newFound, tileId))
                {
                    yield return nextFound;
                }
            }
        }

        private class MatchingTile
        {
            public int Id { get; }
            public Edge Edge { get; }
            public Transformation Tr { get; }

            public MatchingTile(int id, Edge edge, Transformation tr)
            {
                Id = id;
                Edge = edge;
                Tr = tr;
            }
        }
    }
    
    public class ArrangedTiles
    {
        private readonly Dictionary<Vector, TilePosition> tiles;
        private readonly Dictionary<int, Vector> ids;

        public int Count => tiles.Count;

        public IReadOnlyDictionary<int, Vector> Ids => ids;

        public ArrangedTiles()
        {
            tiles = new Dictionary<Vector, TilePosition>();
            ids = new Dictionary<int, Vector>();
        }

        public ArrangedTiles(ArrangedTiles old)
        {
            tiles = new Dictionary<Vector, TilePosition>(old.tiles);
            ids = new Dictionary<int, Vector>(old.ids);
        }

        public Vector GetPosition(int id) => ids[id];

        public Transformation GetTransformation(int id) => tiles[ids[id]].Tr;

        public Tile GetTile(int id) => tiles[ids[id]].Tile;

        public void Add(int id, Tile tile, Vector position, Transformation tr)
        {
            if (IsValid(id, tile, position))
            {
                tiles.Add(position, new TilePosition(id, tile, position, tr));
                ids.Add(id, position);
            }
            else
            {
                throw new ArgumentException("Tile conflicts with existing arranged tiles");
            }
        }

        public bool IsValid(int id, Tile tile, Vector position)
        {
            if (ids.ContainsKey(id) || tiles.ContainsKey(position))
            {
                return false;
            }

            foreach (Edge edge in Edges.All)
            {
                var offset = position + edge.Offset();
                if (tiles.TryGetValue(offset, out var adj) &&
                    tile.Border(edge) != adj.Tile.Border(edge.Opposite()))
                {
                    return false;
                }
            }

            return true;
        }

        public IEnumerable<int> CornerIds()
        {
            var minX = tiles.Values.Select(t => t.Position.X).Min();
            var minY = tiles.Values.Select(t => t.Position.Y).Min();
            var maxX = tiles.Values.Select(t => t.Position.X).Max();
            var maxY = tiles.Values.Select(t => t.Position.Y).Max();

            foreach (int y in new [] { minY, maxY })
            {
                foreach (int x in new [] { minX, maxX })
                {
                    yield return tiles.Values
                        .Where(t => t.Position.X == x && t.Position.Y == y)
                        .Single()
                        .Id;
                }
            }
        }

        private struct TilePosition
        {
            public int Id { get; }
            public Tile Tile { get; }
            public Vector Position { get; }
            public Transformation Tr { get; }

            public TilePosition(int id, Tile tile, Vector position, Transformation tr)
            {
                Id = id;
                Tile = tile;
                Position = position;
                Tr = tr;
            }
        }
    }

    public struct Tile : IEquatable<Tile>
    {
        public const int Side = 10;
        public const int Size = Side * Side;

        private const int Threshold = Size / 2;

        private ulong d0;
        private ulong d1;

        public bool this[int index]
        {
            get
            {
                if (index < 0 || index >= Size)
                {
                    throw new IndexOutOfRangeException();
                }
                return (index < Threshold)
                    ? (d0 & (1UL << index)) > 0
                    : (d1 & (1UL << (index - Threshold))) > 0;
            }
            private set
            {
                if (index < 0 || index >= Size)
                {
                    throw new IndexOutOfRangeException();
                }
                if (index < Threshold && value)
                {
                    d0 |= 1UL << index;
                }
                else if (index < Threshold)
                {
                    d0 &= ~(1UL << index);
                }
                else if (value)
                {
                    d1 |= 1UL << (index - Threshold);
                }
                else
                {
                    d1 &= ~(1UL << (index - Threshold));
                }
            }
        }

        public bool this[Vector position]
        {
            get
            {
                if (position.X < 0 ||
                    position.X >= Size ||
                    position.Y < 0 ||
                    position.Y >= Size )
                {
                    throw new IndexOutOfRangeException();
                }
                return this[position.X + Side * position.Y];
            }
            private set
            {
                if (position.X < 0 ||
                    position.X >= Size ||
                    position.Y < 0 ||
                    position.Y >= Size )
                {
                    throw new IndexOutOfRangeException();
                }
                this[position.X + Side * position.Y] = value;
            }
        }

        private Tile(ulong d0, ulong d1)
        {
            var half = 1UL << Threshold;

            if (d0 >= half) throw new ArgumentOutOfRangeException(nameof(d0));
            if (d1 >= half) throw new ArgumentOutOfRangeException(nameof(d1));

            this.d0 = d0;
            this.d1 = d1;
        }

        public Tile(IEnumerable<bool> data)
        {
            d0 = 0;
            d1 = 0;
            using var enumerator = data.GetEnumerator();

            for (var i = 0; i < Size; i++)
            {
                if (!enumerator.MoveNext())
                {
                    InvalidArraySize(nameof(data));
                }
                this[i] = enumerator.Current;
            }

            if (enumerator.MoveNext())
            {
                InvalidArraySize(nameof(data));
            }
        }

        private static void InvalidArraySize(string param)
        {
            throw new ArgumentException("Tile expected to be a 10x10 square", param);
        }

        public Tile Transform(Transformation tr)
        {
            Func<Vector, Vector>? transform = null;

            if (tr.HasFlag(Transformation.Horizontal))
            {
                if (transform is null)
                {
                    transform = ReflectHorizontal;
                }
                else
                {
                    transform += ReflectHorizontal;
                }
            }
            if (tr.HasFlag(Transformation.Vertical))
            {
                if (transform is null)
                {
                    transform = ReflectVertical;
                }
                else
                {
                    transform += ReflectVertical;
                }
            }
            if (tr.HasFlag(Transformation.Rotation))
            {
                if (transform is null)
                {
                    transform = Rotate;
                }
                else
                {
                    transform += Rotate;
                }
            }
            
            return (transform is null) ? this : ApplyTransform(transform);
        }

        private Tile ApplyTransform(Func<Vector, Vector> transform)
        {
            Tile tile = default;
            for (var i = 0; i < Size; i++)
            {
                if (this[i])
                {
                    var p0 = new Vector(i % Side, i / Side);
                    var p1 = transform(p0);
                    tile[p1] = true;
                }
            }

            return tile;
        }

        private static Vector ReflectHorizontal(Vector position) =>
            new Vector(Side - position.X - 1, position.Y);

        private static Vector ReflectVertical(Vector position) =>
            new Vector(position.X, Side - position.Y - 1);

        private static Vector Rotate(Vector position)
        {
            // Rotate 90 degrees clockwise.
            // Using the 2D rotation matrix, offsetting the position such that the centre of
            // rotation is in middle of the tile and simplifying with sin(90) = 1 and
            // cos(90) = 0, we get the following
            return new Vector(Side - position.Y - 1, position.X);
        }

        public IEnumerable<(Edge Edge, Transformation Tr)> Matches(Tile other)
        {
            var borders = Edges.All.Select(Border).ToArray();

            foreach (Transformation tr in Transformations.All)
            {
                var transformed = other.Transform(tr);
                foreach (Edge edge in Edges.All)
                {
                    var otherBorder = transformed.Border(edge.Opposite());
                    if (borders[(int)edge] == otherBorder)
                    {
                        yield return (edge, tr);
                    }
                }
            }
        }

        public ushort Border(Edge edge)
        {
            var initial = Enumerable.Range(0, Side);
            var indices = edge switch
            {
                Edge.Top => initial,
                Edge.Right => initial.Select(i => Side - 1 + (i * Side)),
                Edge.Bottom => initial.Select(i => Size - Side + i),
                Edge.Left => initial.Select(i => i * Side),
                _ => throw new ArgumentOutOfRangeException(nameof(edge)),
            };
            return BorderInt(indices);
        }

        private ushort BorderInt(IEnumerable<int> indices)
        {
            var value = 0;
            var enumerator = indices.GetEnumerator();
            for (var i = 0; i < Side; i++)
            {
                if (!enumerator.MoveNext())
                {
                    throw new Exception("Expected 10 elements");
                }
                if (this[enumerator.Current])
                {
                    value |= 1 << i;
                }
            }

            return (ushort)value;
        }

        public string Print()
        {
            var builder = new StringBuilder();
            for (var j = 0; j < Side; j++)
            {
                for (var i = 0; i < Side; i++)
                {
                    builder.Append(this[new Vector(i, j)] ? '#' : '.');
                }
                builder.AppendLine();
            }

            return builder.ToString();
        }

        public override int GetHashCode() => (d0, d1).GetHashCode();

        public override bool Equals(object? obj) => obj is Tile t && Equals(t);

        public bool Equals(Tile other) => d0 == other.d0 && d1 == other.d1;
    }

    public enum Edge : byte
    {
        Top,
        Right,
        Bottom,
        Left,
    }

    public static class Edges
    {
        public static readonly IEnumerable<Edge> All = (Edge[])Enum.GetValues(typeof(Edge));

        private static readonly Vector[] Offsets = new []
        {
            new Vector(0, -1),
            new Vector(1, 0),
            new Vector(0, 1),
            new Vector(-1, 0),
        };

        public static Edge Opposite(this Edge edge) => (Edge)(((int)edge + 2) % 4);

        public static Edge Transform(this Edge edge, Transformation tr)
        {
            var e = edge;

            if (tr.HasFlag(Transformation.Horizontal))
            {
                e = e switch
                {
                    Edge.Right => Edge.Left,
                    Edge.Left => Edge.Right,
                    _ => e,
                };
            }
            if (tr.HasFlag(Transformation.Vertical))
            {
                e = e switch
                {
                    Edge.Top => Edge.Bottom,
                    Edge.Bottom => Edge.Top,
                    _ => e,
                };
            }
            if (tr.HasFlag(Transformation.Rotation))
            {
                e = (Edge)(((int)e + 1) % 4);
            }

            return e;
        }

        public static Vector Offset(this Edge edge) => Offsets[(int)edge];
    }

    public enum Transformation : byte
    {
        // A square has 8 axes of symmetry: rotation and reflection in horizontal and vertical axes.
        None = 0,
        Horizontal = 1 << 0,
        Vertical = 1 << 1,
        Rotation = 1 << 2,
    }

    public static class Transformations
    {
        public static readonly IEnumerable<Transformation> All =
            Enumerable.Range(0, 1 << 3).Select(i => (Transformation)i).ToArray();

        public static Transformation Combine(this Transformation left, Transformation right)
        {
            var combined = left ^ right;
            if (left.HasFlag(Transformation.Rotation) && right.HasFlag(Transformation.Rotation))
            {
                // Combining two 90 degree rotations will unset the flag as above but flip both
                // horizontal and vertical reflection axes as they also represent a 180 degree
                // rotation.
                combined ^= Transformation.Horizontal | Transformation.Vertical;
            }

            return combined;
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

        public override string ToString() => $"({X}, {Y})";

        public override int GetHashCode() => (X, Y).GetHashCode();

        public override bool Equals(object? obj) => obj is Vector v && Equals(v);

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
}
