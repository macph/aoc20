using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace AOC20.Day02
{
    public class Day02 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day02.txt";

        private static readonly Regex ReadLine = new Regex(@"^(\d+)-(\d+) (\w): (\w+)$");

        public uint Day => 2;

        public object SolvePart1() =>
            ReadPasswordsFromFile()
                .Where(policy => policy.ValidateOld())
                .Count();

        public object SolvePart2() =>
            ReadPasswordsFromFile()
                .Where(policy => policy.ValidateNew())
                .Count();

        private IEnumerable<PasswordPolicy> ReadPasswordsFromFile()
        {
            using var stream = Utils.OpenResource(Resource);

            foreach (string line in Utils.ReadLines(stream))
            {
                var match = ReadLine.Match(line);
                if (!match.Success)
                {
                    throw new Exception($"Line '{line}' is not valid");
                }

                var policy = new PasswordPolicy(
                    byte.Parse(match.Groups[1].Value),
                    byte.Parse(match.Groups[2].Value),
                    match.Groups[3].Value[0],
                    match.Groups[4].Value);

                yield return policy;
            }
        }
        
        private struct PasswordPolicy
        {
            byte Min { get; }
            byte Max { get; }
            char Required { get; }
            string Password { get; }

            public PasswordPolicy(byte min, byte max, char required, string password)
            {
                Min = min;
                Max = max;
                Required = required;
                Password = password;
            }

            public bool ValidateOld()
            {
                // Password must contain no more than the maximum and no less than the minimum of
                // the required character.
                int found = 0;
                foreach (char c in Password)
                {
                    if (c == Required)
                    {
                        found++;
                    }
                    if (found > Max)
                    {
                        return false;
                    }
                }
                return found >= Min;
            }

            public bool ValidateNew() =>
                // Password must contain the required character at exactly one of the minimum and
                // maximum positions (using 1-index).
                Password[Min - 1] == Required ^ Password[Max - 1] == Required;
        }
    }
}
