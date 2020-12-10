using System;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day6
{
    public class Day6 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day06.txt";

        public uint Day => 6;

        public object SolvePart1() =>
            ReadGroupAnswersFromFile(false).Select(CountAnswers).Sum();

        public object SolvePart2() =>
            ReadGroupAnswersFromFile(true).Select(CountAnswers).Sum();

        private IEnumerable<uint> ReadGroupAnswersFromFile(bool all)
        {
            using var stream = Utils.OpenResource(Resource);

            var group = 0U;
            var updated = false;

            foreach (string line in Utils.ReadLines(stream, skipEmpty: false))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    // Groups are separated by blank lines; yield the last group value if updated
                    // and clear before next group.
                    if (updated)
                    {
                        yield return group;
                        group = 0;
                        updated = false;
                    }
                    continue;
                }

                var answers = 0U;
                foreach (char c in line)
                {
                    answers |= 1U << CharIndex(c);
                }

                // If 'all' is true, everyone in group must answer yes for it to count, otherwise
                // only one person needs to answer yes.
                // Always add the first person's answers if not yet updated.
                group = (all && updated) ? group & answers : group | answers;
                updated = true;
            }

            if (updated)
            {
                yield return group;
            }
        }

        private int CharIndex(char c)
        {
            if (c < 'a' || c > 'z')
            {
                throw new ArgumentOutOfRangeException(
                    nameof(c),
                    "Expected character in range 'a'-'z'");
            }
            return c - 'a';
        }

        private int CountAnswers(uint group)
        {
            var sum = 0U;
            for (int i = 0; i < 26; i++)
            {
                sum += 1U & group >> i;
            }

            return (int)sum;
        }
    }
}
