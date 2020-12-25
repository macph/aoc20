using System;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day23
{
    public class Day23 : ISolvable
    {
        private const string Labels = "784235916";

        public uint Day => 23;

        public string Title => "Crab Cups";

        public object SolvePart1()
        {
            var game = new Game(ReadLabels());
            game.Step(100);

            var labels = game.LabelsAfter(1);
            return string.Join(string.Empty, labels.Select(i => i.ToString()));
        }

        public object SolvePart2()
        {
            // Take first 10 numbers as usual then extend up to 1M.
            var starting = ReadLabels().ToArray();
            var max = starting.Max();
            var toMillion = Enumerable.Range(max + 1, 1_000_000 - max);
            
            var game = new Game(Chain(starting, toMillion));
            game.Step(10_000_000);

            return game.LabelsAfter(1).Take(2).Aggregate(1L, (a, b) => a * b);
        }

        private IEnumerable<int> ReadLabels() => Labels.Select(i => i - '0');

        private IEnumerable<T> Chain<T>(IEnumerable<T> first, IEnumerable<T> second)
        {
            foreach (T item in first)
            {
                yield return item;
            }
            foreach (T item in second)
            {
                yield return item;
            }
        }
    }

    public class Game
    {
        private readonly int[] adjacent;
        private int current;
        private int max;

        public Game(IEnumerable<int> list)
        {
            var values = list.ToArray();

            if (values.Contains(0))
            {
                throw new Exception("Value zero not expected");
            }

            current = values[0];
            max = values.Max();

            // We use an array where the indices are the labels and the values are the next values
            // in clockwise order.
            adjacent = new int[max + 1];

            for (int i = 1; i < values.Length; i++)
            {
                var first = values[i - 1];
                var second = values[i];
                adjacent[first] = second;
            }
            // Set final link between last and first labels.
            adjacent[values[^1]] = values[0];
        }

        public void Step(int steps)
        {
            for (var i = 0; i < steps; i++)
            {
                Step();
            }
        }

        public void Step()
        {
            var picked = PickUpAfter(current, 3);
            // Insert after next lower label from current that isn't in the picked labels.
            var next = (current > 1) ? current - 1 : max;
            while (picked.Contains(next))
            {
                next = (next > 1) ? next - 1 : max;
            }
            SetDownAfter(next, picked);
            current = adjacent[current];
        }

        private int[] PickUpAfter(int label, int count)
        {
            if (count < 0 || count >= adjacent.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            var labels = new int[count];

            var label1 = adjacent[label];
            var label2 = adjacent[label1];

            for (var i = 0; i < count; i++)
            {
                labels[i] = label1;
                adjacent[label] = label2;
                label1 = label2;
                label2 = adjacent[label1];
            }

            return labels;
        }

        private void SetDownAfter(int label, IEnumerable<int> labels)
        {
            var label0 = label;
            var label1 = adjacent[label];

            foreach (int next in labels)
            {
                adjacent[label0] = next;
                adjacent[next] = label1;
                label0 = next;
            }
        }

        public IEnumerable<int> LabelsAfter(int label)
        {
            var label0 = adjacent[label];

            while (label0 != label)
            {
                yield return label0;
                label0 = adjacent[label0];
            }
        }
    }
}
