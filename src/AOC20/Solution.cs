using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace AOC20
{
    public class Solution : IEnumerable<Result>
    {
        public uint Day { get; }
        public object? Result1 { get; }
        public TimeSpan Elapsed1 { get; }
        public object? Result2 { get; }
        public TimeSpan Elapsed2 { get; }

        public Solution(
            uint day,
            object? result1,
            TimeSpan elapsed1,
            object? result2,
            TimeSpan elapsed2)
        {
            Day = day;
            Elapsed1 = elapsed1;
            Result1 = result1;
            Elapsed2 = elapsed2;
            Result2 = result2;
        }

        public static Solution From(ISolvable solvable)
        {
            Stopwatch watch;
            object? result1;
            TimeSpan elapsed1;
            object? result2;
            TimeSpan elapsed2;

            watch = Stopwatch.StartNew();
            try
            {
                result1 = solvable.SolvePart1();
            }
            catch (NotImplementedException)
            {
                result1 = null;
                elapsed1 = default;
            }
            elapsed1 = watch.Elapsed;

            watch = Stopwatch.StartNew();
            try
            {
                result2 = solvable.SolvePart2();
            }
            catch (NotImplementedException)
            {
                result2 = null;
            }
            elapsed2 = watch.Elapsed;

            return new Solution(solvable.Day, result1, elapsed1, result2, elapsed2);
        }

        public IEnumerator<Result> GetEnumerator()
        {
            if (Result1 != null) yield return new Result(Day, 1, Result1, Elapsed1);
            if (Result2 != null) yield return new Result(Day, 2, Result2, Elapsed2);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class Result
    {
        public uint Day { get; }
        public uint Part { get; }
        public object Value { get; }
        public TimeSpan Elapsed { get; }

        public Result(uint day, uint part, object value, TimeSpan elapsed)
        {
            Day = day;
            Part = part;
            Value = value;
            Elapsed = elapsed;
        }

        public string FormatElapsed()
        {
            if (Elapsed.Ticks < 2 * TimeSpan.TicksPerMillisecond / 1000)
            {
                // Print nanoseconds
                return $"{Elapsed.Ticks * 1000000 / TimeSpan.TicksPerMillisecond} ns";
            }
            else if (Elapsed.Ticks < 2 * TimeSpan.TicksPerMillisecond)
            {
                // Print microseconds
                return $"{Elapsed.Ticks * 1000 / TimeSpan.TicksPerMillisecond} µs";
            }
            else if (Elapsed.Ticks < 2 * TimeSpan.TicksPerSecond)
            {
                // Print milliseconds
                return $"{Elapsed.Ticks / TimeSpan.TicksPerMillisecond} ms";
            }
            else
            {
                // Print seconds
                return $"{Elapsed.Ticks / TimeSpan.TicksPerSecond} s";
            }
        }
    }
}
