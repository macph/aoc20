using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOC20.Day20
{
    class Day20 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day20.txt";

        private const string Monster = 
            "                  # " +
            "#    ##    ##    ###" +
            " #  #  #  #  #  #   ";
        private const int MonsterHeight = 3;
        private const int MonsterWidth = 20;

        public uint Day => 20;

        public string Title => "Jurassic Jigsaw";

        public object SolvePart1() =>
            ReadTiles().ArrangeTiles().CornerIds().Aggregate(1L, (a, b) => a * b);

        public object SolvePart2()
        {
            var array = Monster.Select(c => c == '#').ToArray();
            var monster = new Image(array, MonsterHeight, MonsterWidth);

            var image = ReadTiles().ArrangeTiles().CreateImage();
            image.ExcludeImage(monster);

            return image.Count;
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

                var pixels = new List<bool>(TileSet.Size);
                while (lines.MoveNext() && !string.IsNullOrWhiteSpace(lines.Current))
                {
                    pixels.AddRange(lines.Current.Select(ParsePixel));
                }

                set.Add(id, new Image(pixels, TileSet.Side, TileSet.Side));
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
        public const int Side = 10;
        public const int Size = Side * Side;

        private readonly Dictionary<int, Image> tiles;

        public TileSet()
        {
            tiles = new Dictionary<int, Image>();
        }

        public void Add(int id, Image tile)
        {
            if (tile.Height != Side || tile.Width != Side)
            {
                throw new ArgumentException("Expected a 12 x 12 image");
            }
            tiles.Add(id, tile);
        }

        public ArrangedTiles ArrangeTiles()
        {
            var found = new ArrangedTiles();

            var first = tiles.Keys.First();
            found.Add(first, tiles[first], default, Transformation.None);

            var queue = new Queue<int>();
            queue.Enqueue(first);

            while (queue.Count > 0)
            {
                var next = queue.Dequeue();
                var position = found.GetPosition(next);
                var tr = found.GetTransformation(next);

                // Find all adjacent tiles with matching borders.
                // We're making the assumption that every pair of adjacent edges is unique and we
                // don't need to branch out to check every possible combination.
                var matches = tiles.Keys
                    .Where(other => other != next)
                    .SelectMany(other =>
                        tiles[next]
                            .Matches(tiles[other])
                            .Select(p => new MatchingTile(other, p.Edge, p.Tr)))
                    .ToArray();

                foreach (MatchingTile match in matches)
                {
                    if (found.ContainsId(match.Id))
                    {
                        // Tile was inserted already, skip.
                        continue;
                    }
                    // Each adjacent tile was transformed relative to this tile before
                    // transformation so we need to compose the transformations and set the edge
                    // correctly.
                    var tileTr = Transformations.Compose(match.Tr, tr);
                    var tile = tiles[match.Id].Transform(tileTr);
                    var tilePosition = position + match.Edge.Transform(tr).Offset();

                    found.Add(match.Id, tile, tilePosition, tileTr);
                    queue.Enqueue(match.Id);
                }
            }

            return found;
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
        private const int Side = TileSet.Side;
        private const int Size = TileSet.Size;

        private readonly Dictionary<Vector, TilePosition> tiles;
        private readonly Dictionary<int, Vector> ids;

        public int Count => tiles.Count;

        public int MinX => tiles.Values.Select(t => t.Position.X).Min();

        public int MinY => tiles.Values.Select(t => t.Position.Y).Min();

        public int MaxX => tiles.Values.Select(t => t.Position.X).Max();

        public int MaxY => tiles.Values.Select(t => t.Position.Y).Max();

        public int[] XRange => new[] { MinX, MaxX };

        public int[] YRange => new[] { MinY, MaxY };

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

        public bool ContainsPosition(Vector position) => tiles.ContainsKey(position);

        public bool ContainsId(int id) => ids.ContainsKey(id);

        public Vector GetPosition(int id) => ids[id];

        public Transformation GetTransformation(int id) => tiles[ids[id]].Tr;

        public Image GetTile(int id) => tiles[ids[id]].Tile;

        public void Add(int id, Image tile, Vector position, Transformation tr)
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

        private bool IsValid(int id, Image tile, Vector position)
        {
            if (ids.ContainsKey(id) || tiles.ContainsKey(position))
            {
                return false;
            }

            foreach (Edge edge in Edges.All)
            {
                var offset = position + edge.Offset();
                if (tiles.TryGetValue(offset, out var adj) &&
                    !Enumerable.SequenceEqual(tile.Border(edge), adj.Tile.Border(edge.Opposite())))
                {
                    return false;
                }
            }

            return true;
        }

        public IEnumerable<int> CornerIds()
        {
            var xRange = XRange;
            return YRange
                .SelectMany(y => xRange.Select(x => new Vector(x, y)))
                .Select(p => tiles.Values.Where(t => t.Position == p).Single().Id);
        }

        public Image CreateImage()
        {
            var length = 12;
            var size = length * length;
            var xRange = XRange;
            var yRange = YRange;

            if (Count != size ||
                xRange[1] - xRange[0] != length - 1 ||
                yRange[1] - yRange[0] != length - 1)
            {
                throw new Exception($"Expected a {length} * {length} square of tiles");
            }

            var arrayTiles = new Image[size];

            var a = 0;
            for (var j = yRange[0]; j < yRange[1] + 1; j++)
            {
                for (var i = xRange[0]; i < xRange[1] + 1; i++)
                {
                    arrayTiles[a] = tiles[new Vector(i, j)].Tile;
                    a++;
                }
            }

            // Remove border from each tile before merging.
            var imageTileSide = Side - 2;
            var imageSide = imageTileSide * length;
            var imageSize = imageTileSide * imageTileSide * size;

            var image = new bool[imageSize];

            for (var k = 0; k < size; k++)
            {
                var t = arrayTiles[k];
                var i0 = imageTileSide * (k % length);
                var j0 = imageTileSide * (k / length);

                for (var j = 1; j < Side - 1; j++)
                {
                    for (var i = 1; i < Side - 1; i++)
                    {
                        var i1 = i0 + i - 1;
                        var j1 = j0 + j - 1;
                        image[i1 + imageSide * j1] = t[i + Side * j];
                    }
                }
            }

            return new Image(image, imageSide, imageSide);
        }

        private struct TilePosition
        {
            public int Id { get; }
            public Image Tile { get; }
            public Vector Position { get; }
            public Transformation Tr { get; }

            public TilePosition(int id, Image tile, Vector position, Transformation tr)
            {
                Id = id;
                Tile = tile;
                Position = position;
                Tr = tr;
            }
        }
    }

    public class Image
    {
        private readonly bool[] data;
        private readonly int height;
        private readonly int width;

        public int Height => height;

        public int Width => width;

        public int Size => height * width;

        public int Count => data.Where(i => i).Count();

        public bool this[int index]
        {
            get => data[index];
            private set => data[index] = value;
        }

        public bool this[Vector position]
        {
            get => data[position.AsIndex(width)];
            private set => data[position.AsIndex(width)] = value;
        }

        public Image(IEnumerable<bool> data, int height, int width)
        {
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));

            this.data = data.ToArray();
            if (this.data.Length != height * width)
            {
                throw new ArgumentException(
                    $"Expected image to be {height} * {width} = {height * width} elements");
            }

            this.height = height;
            this.width = width;
        }

        private Image(bool[] data, int width)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (data.Length % width != 0)
            {
                throw new ArgumentException($"Expected image to be a multiple of {width} elements");
            }

            this.data = data;
            this.height = this.data.Length / width;
            this.width = width;
        }

        public Image Transform(Transformation tr)
        {
            // Use the distance between min and max indices.
            var transform = tr.GetTransform(height - 1, width - 1);

            if (transform is null)
            {
                return this;
            }

            // Swap height and width if rotating.
            var newWidth = tr.HasFlag(Transformation.Rotation) ? height : width;
            var newData = new bool[Size];

            for (var i = 0; i < Size; i++)
            {
                if (data[i])
                {
                    var p0 = Vector.FromIndex(i, width);
                    var p1 = transform(p0);
                    var j = p1.AsIndex(newWidth);
                    newData[j] = true;
                }
            }

            return new Image(newData, newWidth);
        }

        public int ExcludeImage(Image image)
        {
            if (image.width > width || image.height > height)
            {
                throw new ArgumentException("Inner image must not exceed this image's dimensions");
            }

            var found = 0;
            var indices = new HashSet<int>();

            foreach (Transformation tr in Transformations.All)
            {
                var transformed = image.Transform(tr);
                // Work on a smaller section of outer image such that the inner image will not go
                // out of bounds.
                var leftHeight = height - transformed.height + 1;
                var leftWidth = width - transformed.width + 1;

                for (var j = 0; j < leftHeight; j++)
                {
                    for (var i = 0; i < leftWidth; i++)
                    {
                        var p0 = new Vector(i, j);
                        var innerIndices = Enumerable.Range(0, transformed.Size)
                            .Where(k => transformed.data[k])
                            .Select(k => Vector.FromIndex(k, transformed.width))
                            .Select(p1 => (p0 + p1).AsIndex(width));

                        // Check if every pixel in inner image relative to p0 exists in the outer
                        // image.
                        if (innerIndices.All(j => data[j]))
                        {
                            indices.UnionWith(innerIndices);
                            found++;
                        }
                    }
                }
            }

            // Unset every pixel found.
            foreach (int i in indices)
            {
                data[i] = false;
            }

            return found;
        }

        public IEnumerable<(Edge Edge, Transformation Tr)> Matches(Image other)
        {
            var borders = Edges.All.Select(e => Border(e).ToArray()).ToArray();

            foreach (Transformation tr in Transformations.All)
            {
                var transformed = other.Transform(tr);
                foreach (Edge edge in Edges.All)
                {
                    var otherBorder = transformed.Border(edge.Opposite());
                    if (Enumerable.SequenceEqual(borders[(int)edge], otherBorder))
                    {
                        yield return (edge, tr);
                    }
                }
            }
        }

        public IEnumerable<bool> Border(Edge edge)
        {
            if (height > 32 || width > 32)
            {
                throw new InvalidOperationException("Image too large for integer border values");
            }

            var indices = edge switch
            {
                Edge.Top => Enumerable.Range(0, width),
                Edge.Right => Enumerable.Range(0, height).Select(i => width * (i + 1) - 1),
                Edge.Bottom => Enumerable.Range(0, width).Select(i => Size - width + i),
                Edge.Left => Enumerable.Range(0, height).Select(i => i * width),
                _ => throw new ArgumentOutOfRangeException(nameof(edge)),
            };

            return indices.Select(i => data[i]);
        }
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

        public static Transformation Compose(Transformation left, Transformation right)
        {
            // The group of transformations is not abelian, that is, applying the 3 transformations
            // in different orders do not produce the same result.
            // Composing left with right transformations gives this Cayley table:
            //   right _   h   v   hv  r   hr  vr  hvr
            // left
            // _       _   h   v   hv  r   hr  vr  hvr
            // h       h   _   hv  v   hr  r   hvr hvr
            // v       v   hv  _   h   vr  hvr r   hr
            // hv      hv  v   h   _   hvr vr  hr  r
            // r       r   vr  hr  hvr hv  h   v   _
            // hr      hr  hvr r   vr  v   _   hv  h
            // vr      vr  r   hvr hv  h   hv  _   v
            // hvr     hvr hr   vr  r  _   v   h   hv

            if (left.HasFlag(Transformation.Rotation))
            {
                // Remove rotation from left and apply to right transformation.
                var left0 = left & (Transformation)3;
                var right0 = (int)right switch
                {
                    0 => (Transformation)4,
                    1 => (Transformation)6,
                    2 => (Transformation)5,
                    3 => (Transformation)7,
                    4 => (Transformation)3,
                    5 => (Transformation)1,
                    6 => (Transformation)2,
                    7 => (Transformation)0,
                    _ => throw new ArgumentOutOfRangeException(nameof(right)),
                };
                return left0 ^ right0;
            }
            else
            {
                // Horizontal and vertical reflections are symmetrical and rotation comes last.
                return left ^ right;
            }
        }

        public static Func<Vector, Vector>? GetTransform(
            this Transformation tr,
            int length)
        {
            return GetTransform(tr, length, length);
        }

        public static Func<Vector, Vector>? GetTransform(
            this Transformation tr,
            int height,
            int width)
        {
            // The width/height is between the first and last indices in the rectangle.
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            // Transformations must always be done in the same order every time:
            // horizontal and vertical reflection then rotate 90 degrees clockwise.
            return (int)tr switch
            {
                // No transformation, return null.
                0 => null,
                1 => p => new Vector(width - p.X, p.Y),
                2 => p => new Vector(p.X, height - p.Y),
                3 => p => new Vector(width - p.X, height - p.Y),
                4 => p => new Vector(height - p.Y, p.X),
                5 => p => new Vector(height - p.Y, width - p.X),
                6 => p => new Vector(p.Y, p.X),
                7 => p => new Vector(p.Y, width - p.X),
                _ => throw new ArgumentOutOfRangeException(nameof(tr)),
            };
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

        public static Vector FromIndex(int index, int width)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));

            return new Vector(index % width, index / width);
        }

        public int AsIndex(int width)
        {
            if (X >= width) throw new ArgumentException("X value exceeds width", nameof(width));

            return X + width * Y;
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
