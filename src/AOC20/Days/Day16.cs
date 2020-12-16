using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day16
{
    public class Day16 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day16.txt";

        public uint Day => 16;

        public object SolvePart1() => ReadTicketData().InvalidTicketValues().Sum();

        public object SolvePart2()
        {
            var data = ReadTicketData();
            data.FindTicketFields();
            // Product of fields expected to overflow int, use long instead.
            return data.Fields
                .Where(f => f.StartsWith("departure"))
                .Select(f => data.GetTicketValue(f))
                .Aggregate(1L, (a, b) => a * b);
        }

        private TicketData ReadTicketData()
        {
            using var stream = Utils.OpenResource(Resource);
            var lines = Utils.ReadLines(stream, skipEmpty: false).ToArray();

            return TicketData.ParseFromLines(lines);
        }

        private class TicketData
        {
            private readonly TicketRules rules;
            private readonly int[] ticket;
            private int[][] nearbyTickets;
            private string[]? ticketFields;

            public IEnumerable<string> Fields =>
                ticketFields ?? throw new InvalidOperationException("Ticket fields not set yet");

            private TicketData(TicketRules rules, int[] ticket, int[][] nearbyTickets)
            {
                this.rules = rules;
                this.ticket = ticket;
                this.nearbyTickets = nearbyTickets;
            }

            public static TicketData ParseFromLines(IEnumerable<string> lines)
            {
                // Ticket data expected to be in 3 parts:
                // - Rules for each ticket value in form 'rule: 0-1 or ...';
                // - Your ticket value in form '0,1,...';
                // - Other ticket values on separate lines.

                var rules = TicketRules.ParseFromLines(
                    lines.TakeWhile(line => !string.IsNullOrWhiteSpace(line)));

                var ticket = ParseIntegers(
                    lines.SkipWhile(line => line != "your ticket:").Skip(1).First());

                var nearbyTickets = lines
                    .SkipWhile(line => line != "nearby tickets:")
                    .Skip(1)
                    .TakeWhile(line => !string.IsNullOrWhiteSpace(line))
                    .Select(ParseIntegers)
                    .ToArray();

                if (nearbyTickets.Any(t => t.Length != ticket.Length))
                {
                    throw new FormatException("All tickets must have the same number of values");
                }

                return new TicketData(rules, ticket, nearbyTickets);
            }

            private static int[] ParseIntegers(string line) =>
                line.Split(",").Select(part => int.Parse(part)).ToArray();

            public IEnumerable<int> InvalidTicketValues() =>
                nearbyTickets.SelectMany(value => value).Where(value => !rules.ValueIsValid(value));

            private bool TicketIsValid(int[] ticket) =>
                ticket.All(value => rules.ValueIsValid(value));

            public void FindTicketFields()
            {
                if (ticketFields != null) return;

                // Discard all invalid tickets, ie those with values that don't match any rule.
                DiscardInvalidTickets();

                var length = ticket.Length;

                // Since some fields can match multiple rules we collect rules for which all nearby
                // tickets' values match before deduplicating them.
                var matchingFields = new HashSet<string>[length];
                for (var i = 0; i < length; i++)
                {
                    var matchingRules = rules.Fields
                        .Where(field =>
                            nearbyTickets.All(ticket =>
                                rules.ValueIsValid(field, ticket[i])));
                    matchingFields[i] = new HashSet<string>(matchingRules);
                }

                // For each field with a single rule the rule is removed from all other fields.
                // Repeat until every field has a single matching rule.
                while (matchingFields.Any(s => s.Count > 1))
                {
                    var count = matchingFields.Select(s => s.Count).Sum();
                    for (var i = 0; i < length; i++)
                    {
                        if (matchingFields[i].Count != 1) continue;

                        for (var j = 0; j < length; j++)
                        {
                            if (i == j) continue;
                            matchingFields[j].ExceptWith(matchingFields[i]);
                        }
                    }
                    var newCount = matchingFields.Select(s => s.Count).Sum();
                    if (count == newCount)
                    {
                        throw new Exception("No duplicate fields were removed");
                    }
                }

                // Every field should have a single rule.
                var fields = matchingFields.Select(s => s.Single()).ToArray();

                ticketFields = fields;
            }

            private void DiscardInvalidTickets()
            {
                if (!TicketIsValid(ticket))
                {
                    throw new Exception("Your ticket is not valid");
                }
                nearbyTickets = nearbyTickets.Where(TicketIsValid).ToArray();
            }

            public int GetTicketValue(string field)
            {
                if (ticketFields is null)
                {
                    throw new InvalidOperationException("Ticket fields not set yet");
                }

                var index = Array.IndexOf(ticketFields, field);

                return (index >= 0)
                    ? ticket[index]
                    : throw new ArgumentException("Field does not exist", nameof(field));
            }
        }

        private class TicketRules
        {
            private readonly Dictionary<string, RangeSet> rules;

            public IEnumerable<string> Fields => rules.Keys;

            private TicketRules(Dictionary<string, RangeSet> rules)
            {
                this.rules = rules;
            }

            public static TicketRules ParseFromLines(IEnumerable<string> lines)
            {

                var rules = new Dictionary<string, RangeSet>();
                foreach (string line in lines)
                {
                    var split = line.Split(": ");
                    if (split.Length != 2)
                    {
                        throw new FormatException(
                            "Expected each line to have format 'rule: ranges'");
                    }
                    rules.Add(split[0], RangeSet.Parse(split[1]));
                }

                return new TicketRules(rules);
            }

            public bool ValueIsValid(string field, int value) => rules[field].Contains(value);

            public bool ValueIsValid(int value) =>
                rules.Values.Any(set => set.Contains(value));
        }

        private class RangeSet : ICollection<int>
        {
            private readonly InclusiveRange[] ranges;

            public int Count => ranges.Sum(r => r.Count);

            public bool IsReadOnly => false;

            public RangeSet(IEnumerable<InclusiveRange> ranges)
            {
                this.ranges = ranges.ToArray();
                Array.Sort(this.ranges);

                for (var i = 0; i < this.ranges.Length - 1; i++)
                {
                    for (var j = i + 1; j < this.ranges.Length; j++)
                    {
                        var left = this.ranges[i];
                        var right = this.ranges[j];
                        if (left.Overlaps(right))
                        {
                            throw new ArgumentException(
                                $"All ranges in set must be exclusive, got {left} and {right}");
                        }
                    }
                }
            }

            public static RangeSet Parse(string input) =>
                new RangeSet(input.Split("or").Select(part => InclusiveRange.Parse(part.Trim())));

            public bool Contains(int item) => ranges.Any(r => r.Contains(item));

            public IEnumerator<int> GetEnumerator() => ranges.SelectMany(i => i).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public void Add(int item) => throw new NotImplementedException();

            public void Clear() => throw new NotImplementedException();

            public bool Remove(int item) => throw new NotImplementedException();

            public void CopyTo(int[] array, int arrayIndex) => throw new NotImplementedException();
        }

        private struct InclusiveRange
            : ICollection<int>, IComparable<InclusiveRange>, IEquatable<InclusiveRange>
        {
            public int Min { get; }
            
            public int Max { get; }

            public int Count => Max - Min + 1;

            public bool IsReadOnly => true;

            public InclusiveRange(int min, int max)
            {
                if (min > max)
                {
                    throw new ArgumentException($"Expected minimum <= max, got [{min}, {max}]");
                }
                Min = min;
                Max = max;
            }

            public static InclusiveRange Parse(string input)
            {
                var split = input.Split("-");
                if (split.Length != 2)
                {
                    throw new FormatException("Expected a single '-' character in range");
                }
                var min = int.Parse(split[0].Trim());
                var max = int.Parse(split[1].Trim());

                return new InclusiveRange(min, max);
            }

            public bool Contains(int item) => item >= Min && item <= Max;

            public IEnumerator<int> GetEnumerator() => Enumerable.Range(Min, Count).GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public bool Overlaps(InclusiveRange other) =>
                Contains(other.Min) || Contains(other.Max) || other.Contains(Min);

            public void Add(int item) => throw new NotImplementedException();

            public void Clear() => throw new NotImplementedException();

            public bool Remove(int item) => throw new NotImplementedException();

            public void CopyTo(int[] array, int arrayIndex) => throw new NotImplementedException();

            public int CompareTo (InclusiveRange other)
            {
                var compareMin = Min.CompareTo(other.Min);
                return compareMin != 0 ? compareMin : Max.CompareTo(other.Max);
            }

            public override string ToString() => $"[{Min}, {Max}]";

            public override int GetHashCode() => (Min, Max).GetHashCode();

            public override bool Equals(object? obj) => obj is InclusiveRange && Equals(obj);

            public bool Equals(InclusiveRange other) => Min == other.Min && Max == other.Max;
        }
    }
}
