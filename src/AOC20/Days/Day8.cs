using System;
using System.Collections.Generic;
using System.Linq;

namespace AOC20.Day8
{
    class Day8 : ISolvable
    {
        private const string Resource = "AOC20.Resources.Day8.txt";

        public uint Day => 8;

        public object SolvePart1()
        {
            var instructions = ReadInstructions().ToList();
            RunBootCode(instructions, out var value);
            return value;
        }

        public object SolvePart2()
        {
            var instructions = ReadInstructions().ToList();
            
            for (var i = 0; i < instructions.Count; i++)
            {
                var original = instructions[i];
                // Try switching Jump with NoOp or vice versa, and leave Acc alone.
                switch (original.Operation)
                {
                    case Operation.Acculumate:
                        continue;
                    case Operation.Jump:
                        instructions[i] = new Instruction(Operation.NoOperation, original.Argument);
                        break;
                    case Operation.NoOperation:
                        instructions[i] = new Instruction(Operation.Jump, original.Argument);
                        break;
                    default:
                        throw new Exception($"Unexpected operation: {original.Operation}");
                }

                // Run the modified boot code.
                if (RunBootCode(instructions, out var value))
                {
                    // Boot code ran successfully; return the acculumator value.
                    return value;
                }
                else
                {
                    // Reverse change to instructions.
                    instructions[i] = original;
                }
            }

            throw new Exception("Boot code could not be fixed");
        }

        private bool RunBootCode(IList<Instruction> instructions, out int acculumator)
        {
            acculumator = 0;
            var position = 0;
            var history = new HashSet<int>();

            while (true)
            {
                if (position == instructions.Count)
                {
                    // Boot code completes after position is moved to just after last instruction.
                    return true;
                }
                if (history.Contains(position))
                {
                    // This is an infinite loop.
                    return false;
                }

                history.Add(position);

                var instruction = instructions[position];
                switch (instruction.Operation)
                {
                    case Operation.Acculumate:
                        position += 1;
                        acculumator += instruction.Argument;
                        break;
                    case Operation.Jump:
                        position += instruction.Argument;
                        break;
                    case Operation.NoOperation:
                        position += 1;
                        break;
                    default:
                        throw new Exception($"Unexpected operation: {instruction.Operation}");
                }
            }
        }

        private IEnumerable<Instruction> ReadInstructions()
        {
            using var stream = Utils.OpenResource(Resource);
            foreach (string line in Utils.ReadLines(stream))
            {
                yield return Instruction.Parse(line);
            }
        }

        private struct Instruction : IEquatable<Instruction>
        {
            public Operation Operation { get; }

            public int Argument { get; }

            public Instruction(Operation operation, int argument)
            {
                Operation = operation;
                Argument = argument;
            }

            public static Instruction Parse(string input)
            {
                var words = input.Split();
                if (words.Length != 2)
                {
                    throw new FormatException($"Expected operation and argument, got '{input}'");
                }

                var operation = words[0] switch
                {
                    "acc" => Operation.Acculumate,
                    "jmp" => Operation.Jump,
                    "nop" => Operation.NoOperation,
                    var w => throw new FormatException($"Expected valid operation, got '{w}'"),
                };
                var argument = int.Parse(words[1]);

                return new Instruction(operation, argument);
            }

            public override int GetHashCode() => (Operation, Argument).GetHashCode();

            public override bool Equals(object? other) => other is Instruction && Equals(other);

            public bool Equals(Instruction other) =>
                Operation == other.Operation && Argument == other.Argument;
        }

        private enum Operation : byte
        {
            Acculumate = 0,
            Jump = 1,
            NoOperation = 2,
        }
    }
}
