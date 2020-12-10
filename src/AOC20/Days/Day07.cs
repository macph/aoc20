using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;

namespace AOC20.Day07
{
    public class Day07 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day07.txt";

        public uint Day => 7;

        public object SolvePart1()
        {
            var rules = new RuleCollection(ReadRulesFromFile());
            return rules.Colors
                .Where(color => rules.ContainsAtLeastOne(color, "shiny gold"))
                .Count();
        }

        public object SolvePart2()
        {
            var rules = new RuleCollection(ReadRulesFromFile());
            return rules.CountBagsInside("shiny gold");
        }

        private IEnumerable<Rule> ReadRulesFromFile()
        {
            using var stream = Utils.OpenResource(Resource);
            foreach (string line in Utils.ReadLines(stream))
            {
                yield return Rule.Parse(line);
            }
        }

        private class RuleCollection
        {
            private readonly List<Rule> rules;

            public IEnumerable<Rule> Rules => rules;

            public IEnumerable<string> Colors => rules.Select(r => r.Color);

            public Rule this[string value]
            {
                get
                {
                    var index = IndexOfRule(value);
                    return (index >= 0)
                        ? rules[index]
                        : throw new KeyNotFoundException($"Color '{value}' not found");
                }
            }

            public RuleCollection(IEnumerable<Rule> rules)
            {
                this.rules = rules.ToList();
                this.rules.Sort();

                for (int i = 1; i < this.rules.Count; i++)
                {
                    if (this.rules[i - 1].CompareTo(this.rules[i]) == 0)
                    {
                        throw new Exception("All bags in rule must be unique");
                    }
                }
            }

            public bool ContainsAtLeastOne(string color, string required) =>
                // Search bags for a bag with the required color recursively.
                this[color].ContainsColor(required) ||
                this[color].Colors.Any(inner => ContainsAtLeastOne(inner, required));

            public int CountBagsInside(string color) =>
                // Expand each bag in rule and calculate the required bags recursively.
                this[color].Bags
                    .Select(b => b.Quantity * (1 + CountBagsInside(b.Color)))
                    .Sum();

            private int IndexOfRule(string color)
            {
                if (rules.Count == 0) return -1;

                var low = 0;
                var high = rules.Count - 1;

                while (low <= high)
                {
                    var mid = (low + high) / 2;
                    var cmp = rules[mid].Color.CompareTo(color);

                    if (cmp < 0)
                    {
                        low = mid + 1;
                    }
                    else if (cmp > 0)
                    {
                        high = mid - 1;
                    }
                    else
                    {
                        return mid;
                    }
                }

                return -1;
            }
        }

        private class Rule : IComparable<Rule>
        {
            private readonly string color;
            private readonly List<Bag> bags;

            public string Color => color;

            public IEnumerable<Bag> Bags => bags;

            public IEnumerable<string> Colors => bags.Select(b => b.Color);

            public Rule(string color, IEnumerable<Bag> bags)
            {
                this.color = color;
                this.bags = bags.ToList();
                this.bags.Sort();
                
                for (int i = 1; i < this.bags.Count; i++)
                {
                    if (this.bags[i - 1].CompareTo(this.bags[i]) == 0)
                    {
                        throw new Exception("All bags in rule must be unique");
                    }
                }
            }

            public static Rule Parse(string input)
            {
                var match = Regex.Match(input, @"^(.+?) bags contain (.+?).$");
                if (!match.Success)
                {
                    throw new Exception($"Expected a valid rule for bags, got '{input}'");
                }

                var color = match.Groups[1].Value;
                var other = match.Groups[2].Value;
                var bags = (other == "no other bags")
                    ? Enumerable.Empty<Bag>()
                    : other.Split(", ").Select(b => Bag.Parse(b));

                return new Rule(color, bags);
            }

            public bool ContainsColor(string color) => IndexOfColor(color) >= 0;

            private int IndexOfColor(string color)
            {
                if (bags.Count == 0) return -1;

                var low = 0;
                var high = bags.Count - 1;
                
                while (low <= high)
                {
                    var mid = (low + high) / 2;
                    var cmp = bags[mid].Color.CompareTo(color);

                    if (cmp < 0)
                    {
                        low = mid + 1;
                    }
                    else if (cmp > 0)
                    {
                        high = mid - 1;
                    }
                    else
                    {
                        return mid;
                    }
                }

                return -1;
            }

            public int CompareTo([AllowNull] Rule other) =>
                color.CompareTo(other?.color);
        }

        private class Bag : IComparable<Bag>
        {
            public string Color { get; }

            public int Quantity { get; }

            public Bag(string color, int quantity)
            {
                if (quantity <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(quantity));
                }
                Color = color;
                Quantity = quantity;
            }

            public static Bag Parse(string input)
            {
                var match = Regex.Match(input, @"^(\d+) (.+?) bags?$");
                if (!match.Success)
                {
                    throw new Exception($"Expected bag color and quantity, got '{input}'");
                }

                var quantity = int.Parse(match.Groups[1].Value);
                var color = match.Groups[2].Value;

                return new Bag(color, quantity);
            }

            public int CompareTo([AllowNull] Bag other) =>
                Color.CompareTo(other?.Color);
        }
    }
}
