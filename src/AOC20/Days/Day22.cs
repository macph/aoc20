using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AOC20.Day22
{
    public class Day22 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day22.txt";

        public uint Day => 22;

        public string Title => "Crab Combat";

        public object SolvePart1()
        {
            var game = ReadDecks();
            var result = game.PlaySimple();
            var cards = result switch
            {
                Result.Left => game.Left,
                Result.Right => game.Right,
                _ => throw new Exception("Expected left or right winner"),
            };
            return CardValues(cards);
        }

        public object SolvePart2()
        {
            var game = ReadDecks();
            var result = game.PlayRecursive();
            var cards = result switch
            {
                Result.Left => game.Left,
                Result.Right => game.Right,
                _ => throw new Exception("Expected left or right winner"),
            };
            return CardValues(cards);
        }

        private Game ReadDecks()
        {
            using var stream = Utils.OpenResource(Resource);
            var lines = Utils.ReadLines(stream).ToArray();

            var player1 = Array.IndexOf(lines, "Player 1:");
            var player2 = Array.IndexOf(lines, "Player 2:");

            if (player1 < 0 || player2 < 0)
            {
                throw new Exception("Expected player labels");
            }

            var left = new Deck(lines[(player1 + 1)..player2].Select(int.Parse));
            var right = new Deck(lines[(player2 + 1)..].Select(int.Parse));

            return new Game(left, right);
        }

        private int CardValues(IEnumerable<int> cards)
        {
            var array = cards.ToArray();
            return array.Select((card, i) => card * (array.Length - i)).Sum();
        }
    }

    public class Game
    {
        private readonly Deck left;
        private readonly Deck right;
        private HashSet<FrozenGame> previous;

        public IEnumerable<int> Left => left.Cards;

        public IEnumerable<int> Right => right.Cards;

        public Game(Deck left, Deck right)
        {
            this.left = left;
            this.right = right;
            this.previous = new HashSet<FrozenGame>();
        }

        private Game(Game old)
            : this(new Deck(old.left.Cards), new Deck(old.right.Cards))
        {}

        public Result PlaySimple()
        {
            var result = Result.Continue;
            while (result == Result.Continue)
            {
                result = PlayRound();
            }

            return result;
        }

        private Result PlayRound()
        {
            if (left.IsEmpty)
            {
                return Result.Right;
            }
            else if (right.IsEmpty)
            {
                return Result.Left;
            }

            var leftCard = left.Draw();
            var rightCard = right.Draw();

            // Add cards to winner's deck: the winner's card must be inserted first.
            if (leftCard > rightCard)
            {
                left.Add(leftCard);
                left.Add(rightCard);
            }
            else if (leftCard < rightCard)
            {
                right.Add(rightCard);
                right.Add(leftCard);
            }
            else
            {
                throw new Exception("Expected distinct cards in deck");
            }

            return GetResult();
        }

        public Result PlayRecursive()
        {
            var result = Result.Continue;
            while (result == Result.Continue)
            {
                result = PlayRecursiveRound();
            }

            return result;
        }

        private Result PlayRecursiveRound()
        {
            // TODO: Memoize previous rounds across games?
            // It should be possible to records results for later rounds but testing indicates
            // results aren't always the same, not sure why.

            if (left.IsEmpty) return Result.Right;
            if (right.IsEmpty) return Result.Left;

            // Check previous rounds in this game: if the same ordering of cards in both decks
            // occurred in a previous round the left player wins immediately.
            var state = AsFrozen();
            if (!previous.Add(state))
            {
                return Result.Left;
            }

            var leftCard = left.Draw();
            var rightCard = right.Draw();

            Result result;
            if (leftCard <= left.Count && rightCard <= right.Count)
            {
                // Play another game, taking number of remaining cards equal to card value.
                var inner = new Game(
                    new Deck(left.Cards.Take(leftCard)),
                    new Deck(right.Cards.Take(rightCard)));
                result = inner.PlayRecursive();
            }
            else
            {
                // Highest card wins.
                result = leftCard > rightCard
                    ? Result.Left
                    : leftCard < rightCard
                    ? Result.Right
                    : throw new Exception("Expected two distinct cards");
            }

            // Add cards to winner's deck: the winner's card must be inserted first.
            if (result == Result.Left)
            {
                left.Add(leftCard);
                left.Add(rightCard);
            }
            else if (result == Result.Right)
            {
                right.Add(rightCard);
                right.Add(leftCard);
            }
            else
            {
                throw new Exception("Expected left or right winner");
            }

            return GetResult();
        }

        private Result GetResult() =>
            left.IsEmpty ? Result.Right : right.IsEmpty ? Result.Left : Result.Continue;

        private FrozenGame AsFrozen() => new FrozenGame(left.Cards, right.Cards);
    }

    public enum Result
    {
        Continue,
        Left,
        Right,
    }

    public class Deck
    {
        public const int Limit = 10007;

        private readonly Queue<int> inner;

        public int Count => inner.Count;

        public IEnumerable<int> Cards => inner;

        public bool IsEmpty => inner.Count == 0;

        public Deck(IEnumerable<int> cards)
        {
            inner = new Queue<int>();
            foreach (int card in cards)
            {
                Add(card);
            }
        }

        public void Add(int card)
        {
            if (card < 0 || card >= Limit)
            {
                throw new ArgumentOutOfRangeException(nameof(card));
            }
            if (inner.Contains(card))
            {
                throw new InvalidOperationException();
            }
            inner.Enqueue(card);
        }

        public int Draw() => inner.Dequeue();
    }

    public class FrozenGame : IEquatable<FrozenGame>
    {
        private readonly int[] left;
        private readonly int[] right;

        public FrozenGame(IEnumerable<int> left, IEnumerable<int> right)
        {
            this.left = left.ToArray();
            this.right = right.ToArray();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hash = 17;
                hash = AddArrayHash(left, hash);
                hash = AddArrayHash(right, hash);
                return hash;
            }
        }

        private static int AddArrayHash(int[] array, int hash)
        {
            // https://stackoverflow.com/a/263416/12908045
            unchecked
            {
                hash = hash * 23 + array.Length.GetHashCode();
                foreach (int integer in array)
                {
                    hash = hash * 23 + integer.GetHashCode();
                }
                return hash;
            }
        }

        public override bool Equals(object? obj) => obj is FrozenGame other && Equals(other);

        public bool Equals([AllowNull] FrozenGame other) =>
            other is FrozenGame &&
            left.SequenceEqual(other.left) &&
            right.SequenceEqual(other.right);
    }
}
