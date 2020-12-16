using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AOC20.Day04
{
    public class Day04 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day04.txt";

        public uint Day => 4;

        public string Title => "Passport Processing";

        public object SolvePart1() =>
            ReadPassportsFromFile()
                .Where(p => p.CheckFields())
                .Count();

        public object SolvePart2() =>
            ReadPassportsFromFile()
                .Where(p => p.ValidateFields())
                .Count();

        private IEnumerable<Passport> ReadPassportsFromFile()
        {
            var stream = Utils.OpenResource(Resource);
            var current = new Passport();
            var updated = false;

            foreach (string line in Utils.ReadLines(stream, false))
            {
                if (line == string.Empty)
                {
                    // Passports are separated by empty lines; yield current passport and restart.
                    yield return current;
                    current = new Passport();
                    updated = false;
                    continue;
                }
                foreach (string keyValue in line.Split())
                {
                    current.AddKeyValue(keyValue);
                    updated = true;
                }
            }

            if (updated)
            {
                // Yield last passport if it has been updated with new fields, in case the last line
                // was not empty.
                yield return current;
            }
        }

        private class Passport
        {
            static readonly string[] ExpectedFields = {
                "byr", "iyr", "eyr", "hgt", "hcl", "ecl", "pid", "cid",
            };
            static readonly string[] EyeColors = {
                "amb", "blu", "brn", "gry", "grn", "hzl", "oth",
            };

            private Dictionary<string, string> fields;

            public Passport()
            {
                fields = new Dictionary<string, string>();
            }

            public void AddKeyValue(string keyValue)
            {
                var pair = keyValue.Split(':');
                if (pair.Length != 2)
                {
                    throw new ArgumentException(
                        $"Expected a key:value pair with single colon character, got '{keyValue}'",
                        nameof(keyValue));
                }

                if (!ExpectedFields.Contains(pair[0]))
                {
                    throw new Exception($"'{pair[0]}' is not a valid key");
                }

                try
                {
                    fields.Add(pair[0], pair[1]);
                }
                catch (ArgumentException e)
                {
                    throw new Exception($"Field '{pair[0]}' already populated", e);
                }
            }

            public bool CheckFields() =>
                // Country ID ('cid') is optional.
                ExpectedFields
                    .Where(f => f != "cid")
                    .All(f => fields.ContainsKey(f));
            
            public bool ValidateFields() =>
                ValidateBirthYear() &&
                    ValidateIssueYear() &&
                    ValidateExpirationYear() &&
                    ValidateHeight() &&
                    ValidateHairColor() &&
                    ValidateEyeColor() &&
                    ValidatePassportID();

            private bool ValidateBirthYear() =>
                fields.TryGetValue("byr", out var strYear) &&
                    int.TryParse(strYear, out var year) &&
                    year >= 1920 &&
                    year <= 2002;
            
            private bool ValidateIssueYear() =>
                fields.TryGetValue("iyr", out var strYear) &&
                    int.TryParse(strYear, out var year) &&
                    year >= 2010 &&
                    year <= 2020;

            private bool ValidateExpirationYear() =>
                fields.TryGetValue("eyr", out var strYear) &&
                    int.TryParse(strYear, out var year) &&
                    year >= 2020 &&
                    year <= 2030;

            private bool ValidateHeight()
            {
                if (!fields.TryGetValue("hgt", out var height)) return false;

                if (height.EndsWith("cm"))
                {
                    return int.TryParse(height[..^2], out var cm) &&
                        cm >= 150 &&
                        cm <= 193;
                }
                else if (height.EndsWith("in"))
                {
                    return int.TryParse(height[..^2], out var inches) &&
                        inches >= 59 &&
                        inches <= 76;
                }
                else
                {
                    return false;
                }
            }

            private bool ValidateHairColor() =>
                fields.TryGetValue("hcl", out var hairColor) &&
                    hairColor.Length == 7 &&
                    hairColor[0] == '#' &&
                    int.TryParse(hairColor[1..], NumberStyles.HexNumber, null, out var _);

            private bool ValidateEyeColor() =>
                fields.TryGetValue("ecl", out var eyeColor) && EyeColors.Contains(eyeColor);

            private bool ValidatePassportID() =>
                fields.TryGetValue("pid", out var passportId) &&
                    passportId.Length == 9 &&
                    int.TryParse(passportId, out var _);
        }
    }
}
