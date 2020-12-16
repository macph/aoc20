using System;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day14
{
    public class Day14 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day14.txt";

        public uint Day => 14;

        public string Title => "Docking Data";

        public object SolvePart1() => LoadMemory(ReadInstructions(), false);

        public object SolvePart2() => LoadMemory(ReadInstructions(), true);

        private IEnumerable<object> ReadInstructions()
        {
            var stream = Utils.OpenResource(Resource);
            foreach (string line in Utils.ReadLines(stream))
            {
                if (line.StartsWith("mask = "))
                {
                    // Found line of form "mask = ..."
                    yield return Mask.Parse(line[7..]);
                }
                else if (line.StartsWith("mem["))
                {
                    // Found line of form "mem[...] = ..."
                    var split = line[4..].Split("] = ");
                    var address = ulong.Parse(split[0]);
                    var value = ulong.Parse(split[1]);
                    yield return new Assignment(address, value);
                }
                else
                {
                    throw new Exception(
                        $"Expected a bitmask definition or a memory assignment, got '{line}'");
                }
            }
        }

        private ulong LoadMemory(IEnumerable<object> instructions, bool useFloating)
        {
            Mask mask = default;
            var memory = new Dictionary<ulong, ulong>();

            foreach (object i in instructions)
            {
                switch (i)
                {
                    case Mask newMask:
                        mask = newMask;
                        break;
                    case Assignment assignment when useFloating:
                        foreach (ulong address in mask.ApplyFloating(assignment.Address))
                        {
                            memory[address] = assignment.Value;
                        }
                        break;
                    case Assignment assignment:
                        memory[assignment.Address] = mask.Apply(assignment.Value);
                        break;
                    default:
                        throw new Exception(
                            $"Expected a bitmask definition or a memory assignment, got '{i}'");
                }
            }

            return memory.Values.Aggregate((a, b) => a + b);
        }

        private struct Assignment
        {
            public ulong Address { get; }
            public ulong Value { get; }

            public Assignment(ulong address, ulong value)
            {
                Address = address;
                Value = value;
            }
        }

        private struct Mask
        {
            private readonly ulong on;
            private readonly ulong off;
            private readonly ulong floating;

            private Mask(ulong on, ulong off, ulong floating)
            {
                this.on = on;
                this.off = off;
                this.floating = floating;
            }

            public static Mask Parse(string input)
            {
                if (input.Length > 64)
                {
                    throw new FormatException("Mask must not be more than 64 bits");
                }

                var on = 0UL;
                var off = 0UL;
                var floating = 0UL;

                for (var i = 0; i < input.Length; i++)
                {
                    int j = input.Length - i - 1;
                    switch (input[i])
                    {
                        case 'X':
                            floating |= 1UL << j;
                            break;
                        case '0':
                            off |= 1UL << j;
                            break;
                        case '1':
                            on |= 1UL << j;
                            break;
                        case var c:
                            throw new FormatException(
                                $"Expected characters 'X', '0' or '1', got '{c}'");
                    }
                }
                // Set rest of 64 bits to 1 for OFF mask so they will not be applied.
                off |= ~0UL ^ ((1UL << input.Length) - 1UL);

                return new Mask(on, off, floating);
            }

            public ulong Apply(ulong value) => (value | on) & ~off;

            public IEnumerable<ulong> ApplyFloating(ulong address)
            {
                // Set all bits with ON mask and unset bits with original floating mask before
                // applying each floating variant.
                var newAddress = (address | on) & ~floating;
                return ExpandFloatingMask().Select(f => newAddress | f);
            }

            private IEnumerable<ulong> ExpandFloatingMask()
            {
                if (floating == 0)
                {
                    // No possible combinations.
                    yield break;
                }
                // Create array of shifts to translate each bit in combination to position of each
                // bit in floating mask.
                var shifts = FindShifts(floating);

                // Finally, get combination of all possible bits and translate to floating mask
                // bits using array of shifts.
                var total = 1UL << shifts.Length;
                for (var i = 0UL; i < total; i++)
                {
                    var newMask = 0UL;
                    for (var j = 0; j < shifts.Length; j++)
                    {
                        newMask |= (i & 1UL << j) << (shifts[j] - j);
                    }
                    yield return newMask;
                }
            }

            private static int[] FindShifts(ulong mask)
            {
                var bits = CountBits(mask);

                var shifts = new int[bits];
                var i = 0;
                var j = 0;
                while (mask != 0)
                {
                    if ((mask & 1UL) != 0UL)
                    {
                        shifts[i] = j;
                        i++;
                    }
                    j++;
                    mask >>= 1;
                }

                return shifts;
            }

            private static int CountBits(ulong mask)
            {
                int i;
                for (i = 0; mask != 0; i++)
                {
                    mask &= mask - 1;
                }
                return i;
            }
        }
    }
}
