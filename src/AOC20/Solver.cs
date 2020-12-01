using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AOC20
{
    public class SolverSet
    {
        private static readonly SolutionComparer comparer = new SolutionComparer();

        private readonly List<ISolvable> solvers;

        public SolverSet()
        {
            solvers = new List<ISolvable>();
        }

        public SolverSet Register(ISolvable solvable)
        {
            var index = solvers.BinarySearch(solvable, comparer);
            if (index < 0)
            {
                solvers.Insert(~index, solvable);
                return this;
            }
            else
            {
                throw new ArgumentException(
                    $"Solutions already exist for day {solvable.Day}");
            }
        }

        public IEnumerable<Solution> SolveLast() =>
            (solvers.Count > 0)
            ? Single(solvers[^1]).Select(GetSolution)
            : Enumerable.Empty<Solution>();

        public IEnumerable<Solution> Solve(uint? day = null) =>
            (day is null)
            ? solvers.Select(GetSolution)
            : solvers.Where(s => s.Day == day).Select(GetSolution);

        private static Solution GetSolution(ISolvable solvable)
        {
            Stopwatch watch;

            watch = Stopwatch.StartNew();
            var result1 = solvable.SolvePart1();
            var elapsed1 = watch.Elapsed;

            watch = Stopwatch.StartNew();
            var result2 = solvable.SolvePart2();
            var elapsed2 = watch.Elapsed;

            return new Solution(solvable.Day, elapsed1, result1, elapsed2, result2);
        }

        private static IEnumerable<T> Single<T>(T value)
        {
            yield return value;
        }

        private class SolutionComparer : IComparer<ISolvable>
        {
            public int Compare(ISolvable? x, ISolvable? y)
            {
                if (x is null) throw new ArgumentNullException(nameof(x));
                if (y is null) throw new ArgumentNullException(nameof(y));

                return x.Day.CompareTo(y.Day);
            }
        }
    }
}
