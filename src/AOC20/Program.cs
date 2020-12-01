using CommandLine;
using System;
using System.Collections.Generic;

namespace AOC20
{
    class Program
    {
        public class Options
        {
            [Option('d', SetName = "day", HelpText = "Solve problems for a specific day.")]
            public uint? Day { get; }

            [Option('l', SetName = "last", HelpText = "Solve problems for the last day.")]
            public bool Last { get; }

            public Options(uint? day, bool last)
            {
                Day = day;
                Last = last;
            }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args).WithParsed(RunOptions);
        }

        static void RunOptions(Options options)
        {
            var set = CollectSet();
            var solutions = options.Last ? set.SolveLast() : set.Solve(options.Day);
            DisplaySolutions(solutions);
        }

        static SolverSet CollectSet() =>
            new SolverSet()
                .Register(new Day1.Day1());

        static void DisplaySolutions(IEnumerable<Solution> solutions)
        {
            var format = " {0,3} {1,4} {2,24} {3,-40}";
            Console.WriteLine(
                format,
                "Day",
                "Part",
                "Elapsed (ms)",
                "Solution");

            foreach (Solution solution in solutions)
            {
                Console.WriteLine(
                    format,
                    solution.Day,
                    1,
                    solution.Elapsed1.TotalMilliseconds,
                    solution.Result1);
                Console.WriteLine(
                    format,
                    solution.Day,
                    2,
                    solution.Elapsed2.TotalMilliseconds,
                    solution.Result2);
            }
        }
    }
}
