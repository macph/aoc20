using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AOC20
{
    class Program
    {
        public class Options
        {
            [Option('d', "day", SetName = "day", HelpText = "Solve problems for a specific day.")]
            public uint? Day { get; }

            [Option('l', "last", SetName = "last", HelpText = "Solve problems for the last day.")]
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

        private static void RunOptions(Options options)
        {
            // Set up a collection and provider for running solutions with logging.
            var collection = new ServiceCollection();
            ConfigureServices(collection);

            var provider = collection.BuildServiceProvider();

            IEnumerable<ISolvable> toSolve;
            if (options.Last)
            {
                var latest = provider.GetServices<ISolvable>().Select(s => s.Day).Max();
                toSolve = provider.GetServices<ISolvable>().Where(s => s.Day == latest);
            }
            else if (options.Day != null)
            {
                toSolve = provider.GetServices<ISolvable>().Where(s => s.Day == options.Day);
            }
            else
            {
                toSolve = provider.GetServices<ISolvable>();
            }

            var results = toSolve
                .Select(Solution.From)
                .OrderBy(result => result.Day)
                .ToList();

            if (results.Count > 0)
            {
                DisplaySolutions(results);
            }
            else
            {
                Console.WriteLine("No solutions found.");
            }
        }

        private static void ConfigureServices(IServiceCollection collection)
        {
            // Add logging.
            collection.AddLogging(logging =>
                logging
                    .AddConsole()
                    .AddDebug());
            // Add solvers. Since the solutions only need to be run once they can be added as
            // singletons.
            collection
                .AddSingleton<ISolvable, Day1.Day1>()
                .AddSingleton<ISolvable, Day2.Day2>();
        }

        private static void DisplaySolutions(IEnumerable<Solution> solutions)
        {
            var format = "{0,3}  {1,4}  {2,16}  {3,-40}";
            Console.WriteLine(
                format,
                "Day",
                "Part",
                "Elapsed",
                "Solution");

            foreach (Result result in solutions.SelectMany(s => s))
            {
                Console.WriteLine(
                    format,
                    result.Day,
                    result.Part,
                    result.FormatElapsed(),
                    result.Value);
            }
        }
    }
}
