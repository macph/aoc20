using System;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day5
{
    public class Day5 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day05.txt";

        public uint Day => 5;

        public string Title => "Binary Boarding";

        public object SolvePart1() => ReadSeatIdsFromFile().Max();

        public object SolvePart2()
        {
            var occupied = new bool[1 << 10];
            foreach (int id in ReadSeatIdsFromFile())
            {
                if (occupied[id])
                {
                    throw new Exception($"Seat id {id} already occupied");
                }
                occupied[id] = true;
            }

            // The missing seat ID must have seats (ID - 1) and (ID + 1) occupied: can skip the
            // first and last IDs. 
            return Enumerable.Range(1, occupied.Length - 2)
                .Where(id => !occupied[id] && occupied[id - 1] && occupied[id + 1])
                .Single();
        }

        private IEnumerable<int> ReadSeatIdsFromFile()
        {
            using var stream = Utils.OpenResource(Resource);
            foreach (string line in Utils.ReadLines(stream))
            {
                yield return GetSeatId(line);
            }
        }

        private int GetSeatId(string specifier)
        {
            // The specifier uses F/B for the first 7 characters and L/R for the next 3 characters
            // to represent 0/1.
            if (specifier.Length != 10)
            {
                throw new ArgumentException(
                    $"Seat specifier '{specifier}' must be 10 characters in length");
            }

            // Seat ID is defined as 8 * row + column.
            // Construct an integer with the first 3 bits the column and the next 7 bits the row:
            // the result should be identical to the seat ID.
            int id = 0;
            for (int i = 0; i < 10; i++)
            {
                int flag = specifier[i] switch
                {
                    'F' when i < 7 => 0,
                    'B' when i < 7 => 1,
                    'L' when i >= 7 => 0,
                    'R' when i >= 7 => 1,
                    char c => throw new Exception(
                        $"Expected valid format for '{specifier}', got character '{c}'"),
                };
                id |= flag << 9 - i;
            }

            return id;
        }
    }
}
