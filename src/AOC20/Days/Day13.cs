using System;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day13
{
    class Day13 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day13.txt";

        public uint Day => 13;

        public object SolvePart1()
        {
            var (service, time) = ReadTimetable().NextService();
            return service * time;
        }

        public object SolvePart2() => ReadTimetable().FindDepartureSequence();

        private Timetable ReadTimetable()
        {
            using var stream = Utils.OpenResource(Resource);
            return Timetable.ParseFromLines(Utils.ReadLines(stream));
        }

        private class Timetable
        {
            private readonly uint timestamp;
            private readonly uint[] services;

            private Timetable(uint timestamp, uint[] services)
            {
                this.timestamp = timestamp;
                this.services = services;
            }

            public static Timetable ParseFromLines(IEnumerable<string> lines)
            {
                var linesRead = lines.ToArray();
                if (linesRead.Length != 2)
                {
                    throw new FormatException("Expected two lines");
                }

                var timestamp = uint.Parse(linesRead[0]);
                var services = linesRead[1]
                    .Split(',')
                    .Select(s => (s == "x") ? 0 : uint.Parse(s))
                    .ToArray();

                return new Timetable(timestamp, services);
            }

            public (uint Service, uint Time) NextService()
            {
                var found = false;
                uint nextTimestamp = default;
                uint nextService = default;

                // Find the smallest timestamp after the current timestamp for a service.
                foreach (uint service in services)
                {
                    if (service == 0)
                    {
                        // Ignore services numbered zero ("x").
                        continue;
                    }
                    // Find the next service coming after the timestamp.
                    var newTimestamp = (timestamp / service + 1) * service;
                    if (!found || newTimestamp < nextTimestamp)
                    {
                        // New earliest service found.
                        nextTimestamp = newTimestamp;
                        nextService = service;
                    }
                    found = true;
                }

                if (!found)
                {
                    throw new Exception("Expected at least one service in timetable");
                }

                return (Service: nextService, Time: nextTimestamp - timestamp);
            }

            public ulong FindDepartureSequence()
            {
                // Find the LCM (least common multiple) of each pair of numbers. If the numbers are
                // coprime or prime the LCM will simply be a * b.
                // The multiples which are offset by 1, etc will be 'fixed' relative to the LCM.
                // Eg for a = 3 and b = 5 with offset 1:
                //   The LCM is 15:
                //     (5 * i * 3 == 3 * i * 5) for all i.
                //   The multiples we want are 3 * 3 = 9 and 2 * 5 = 10, which can both be
                //   incremented by 15 to preserve the offset:
                //     (3i + 2) * 5 - (5i + 3) * 3 = 15i + 10 - 15i - 9 = 1 for all i.
                // Then for the next number c with another offset we use the above sequence to find
                // the next number satisfying all offsets, and so on.

                // This is better known as the Chinese Remainder Theorem.

                if (services.Length < 1)
                {
                    throw new Exception("Expected at least one services");
                }
                // Start by setting the left and earliest timestamp to the first service in list.
                var first = (ulong)services[0];
                var left = first;
                var earliest = first;

                for (int i = 1; i < services.Length; i++)
                {
                    if (services[i] == 0)
                    {
                        // Ignore services numbered zero ("x").
                        continue;
                    }

                    var updated = false;
                    // The offset includes ignored services.
                    var offset = (ulong)i;
                    var right = (ulong)services[i];
                    var lcm = LCM(left, right);

                    // Find next timestamp preserving earlier offsets.
                    for (ulong multiple = earliest; multiple < lcm; multiple += left)
                    {
                        if ((multiple + offset) % right == 0)
                        {
                            // Update offset and LCM.
                            earliest = multiple;
                            left = lcm;
                            updated = true;
                            break;
                        }
                    }
                    if (!updated)
                    {
                        throw new Exception("New offset not found");
                    }
                }

                return earliest;
            }

            private static ulong LCM(ulong a, ulong b) => a / GCD(a, b) * b;

            private static ulong GCD(ulong a, ulong b) => (b == 0) ? a : GCD(b, a % b);
        }
    }
}
