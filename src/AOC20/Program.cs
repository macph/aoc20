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

            // Pick solutions based on options.
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

            // Run the solvers in order, and display the solutions.
            var results = toSolve
                .OrderBy(solvable => solvable.Day)
                .Select(Solution.From)
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

            // Add solvers using reflection. Since the solutions only need to be run once they can
            // be added as singletons.
            foreach (Type type in EnumSolvableTypes())
            {
                collection.AddSingleton(typeof(ISolvable), type);
            }
        }

        private static IEnumerable<Type> EnumSolvableTypes()
        {
            // No need to search multiple assemblies as this is a single project.
            var asm = typeof(Program).Assembly;
            var itype = typeof(ISolvable);
            // Match all non-abstract types implementing ISolvable.
            return asm.GetTypes().Where(type => !type.IsAbstract && itype.IsAssignableFrom(type));
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
