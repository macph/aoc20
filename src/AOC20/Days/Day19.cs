using System;
using System.Collections.Generic;
using System.Linq;

// TODO: Is there a better way of designing this recursive structure?
// TODO: Reduce terms and concatenate fixed sequences of characters into strings?

namespace AOC20.Day19
{
    class Day19 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day19.txt";

        public uint Day => 19;

        public string Title => "Monster Messages";

        public object SolvePart1() => ReadRules().ValidateAll();

        public object SolvePart2()
        {
            var rules = ReadRules();

            rules.Replace(8, new Choice(new Sequence(42), new Sequence(42, 8)));
            rules.Replace(11, new Choice(new Sequence(42, 31), new Sequence(42, 11, 31)));

            return rules.ValidateAll();
        }

        private RuleSet ReadRules()
        {
            using var stream = Utils.OpenResource(Resource);

            var skipped = false;
            var rules = new List<(int, IRule)>();
            var data = new List<string>();

            foreach (string line in Utils.ReadLines(stream, skipEmpty: false))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    skipped = true;
                    continue;
                }
                if (skipped)
                {
                    data.Add(line);
                }
                else
                {
                    rules.Add(ParseRule(line));
                }
            }

            return new RuleSet(rules, data);
        }

        private class RuleSet
        {
            private readonly Dictionary<int, IRule> rules;
            private readonly string[] data;

            public RuleSet(IEnumerable<(int Id, IRule Rule)> rules, IEnumerable<string> data)
            {
                this.rules = rules.ToDictionary(r => r.Id, r => r.Rule);
                this.data = data.ToArray();
            }

            public void Replace(int id, IRule rule)
            {
                if (!rules.ContainsKey(id))
                {
                    throw new Exception("Rule ID does not exist");
                }
                rules[id] = rule;
            }

            public int ValidateAll()
            {
                if (!rules.TryGetValue(0, out var rule))
                {
                    throw new Exception("Key 0 expected");
                }
                return data.Where(line => Validate(rule, line)).Count();
            }

            public bool Validate(IRule rule, string input)
            {
                // Set recursion depth to line length.
                var depth = input.Length;
                // Get at least one match that covers the entire string.
                return rule.Match(rules, input, 0, depth).Where(i => i >= input.Length).Any();
            }
        }

        private (int, IRule) ParseRule(string input)
        {
            // Expect a rule of the form 'id: id0 id1...'.
            var parts = input.Split(":");
            if (parts.Length != 2)
            {
                throw new FormatException("Expected a single colon");
            }

            int id = int.Parse(parts[0]);
            IRule rule;

            // Split rules using vertical bar as delimiter.
            var either = parts[1].Split("|");
            if (either.Length == 1)
            {
                // Single rule.
                // Expect either a single quoted character to match against or a sequence of IDs.
                var quoted = parts[1].Split("\"");
                if (quoted.Length == 1)
                {
                    // Sequence of rule IDs.
                    rule = ParseSequence(parts[1]);
                }
                else if (quoted.Length == 3)
                {
                    // Single character.
                    rule = new Character(quoted[1][0]);
                }
                else
                {
                    throw new Exception("Expected a quoted character or a pair of integers");
                }
            }
            else
            {
                // Multiple rules, parse each.
                rule = new Choice(either.Select(ParseSequence));
            }

            return (id, rule);
        }

        private IRule ParseSequence(string input) =>
            new Sequence(input.Trim().Split().Select(int.Parse));

        private interface IRule
        {
            // Work recursively: for every possible match return the index for the next character,
            // which can then be evaluated, and so on.
            public IEnumerable<int> Match(
                IDictionary<int, IRule> rules,
                string input,
                int index,
                int depth);
        }

        private class Choice : IRule
        {
            private readonly IRule[] choices;

            public Choice(IEnumerable<IRule> choices)
            {
                this.choices = choices.ToArray();
            }

            public Choice(params IRule[] choices)
            {
                this.choices = choices;
            }

            public IEnumerable<int> Match(
                IDictionary<int, IRule> rules,
                string input,
                int index,
                int depth)
            {
                // Return match for every choice, if depth not exceeded.
                return (depth > 0)
                    ? choices.SelectMany(rule => rule.Match(rules, input, index, depth - 1))
                    : Enumerable.Empty<int>();
            }
        }

        private class Sequence : IRule
        {
            private readonly int[] ids;

            public Sequence(IEnumerable<int> ids)
            {
                this.ids = ids.ToArray();
            }

            public Sequence(params int[] ids)
            {
                this.ids = ids;
            }

            public IEnumerable<int> Match(
                IDictionary<int, IRule> rules,
                string input,
                int index,
                int depth)
            {
                // Return all possible matches satisifying sequence, if depth not exceeded.
                return (depth > 0)
                    ? MatchInner(rules, input, index, depth)
                    : Enumerable.Empty<int>();
            }

            private IEnumerable<int> MatchInner(
                IDictionary<int, IRule> rules,
                string input,
                int index,
                int depth,
                int idIndex = 0)
            {
                // For each match in sequence, expand every subsequent rule and match with them.
                var matched = rules[ids[idIndex]].Match(rules, input, index, depth - 1);
                return (idIndex == ids.Length - 1)
                    ? matched
                    : matched.SelectMany(newIndex =>
                        MatchInner(rules, input, newIndex, depth, idIndex + 1));
            }
        }

        private struct Character : IRule
        {
            private readonly char value;

            public Character(char value)
            {
                this.value = value;
            }

            public IEnumerable<int> Match(
                IDictionary<int, IRule> rules,
                string input,
                int index,
                int depth)
            {
                // Only yield next index if character matches.
                if (index < input.Length && input[index] == value)
                {
                    yield return index + 1;
                }
            }
        }
    }
}
