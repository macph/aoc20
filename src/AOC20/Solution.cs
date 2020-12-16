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
            object? result2;
            TimeSpan elapsed1;
            TimeSpan elapsed2;

            watch = Stopwatch.StartNew();
            try
            {
                result1 = solvable.SolvePart1();
            }
            catch (NotImplementedException)
            {
                result1 = null;
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
        public FormattedTimespan Elapsed { get; }

        public Result(uint day, uint part, object value, TimeSpan elapsed)
        {
            Day = day;
            Part = part;
            Value = value;
            Elapsed = new FormattedTimespan(elapsed);
        }
    }

    public struct FormattedTimespan : IEquatable<FormattedTimespan>
    {
        private const long TicksPerSecond = TimeSpan.TicksPerSecond;
        private const long TicksPerMillisecond = TimeSpan.TicksPerMillisecond;
        private const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;

        private const long Threshold = 2000L;
        private const int Figures = 3;

        private readonly TimeSpan span;

        public FormattedTimespan(TimeSpan span)
        {
            this.span = span;
        }

        public override string ToString()
        {
            if (span.Ticks < Threshold * TicksPerMicrosecond)
            {
                // Print microseconds.
                return $"{RoundTo((double)span.Ticks / TicksPerMicrosecond, Figures)} µs";
            }
            else if (span.Ticks < Threshold * TicksPerMillisecond)
            {
                // Print milliseconds.
                return $"{RoundTo((double)span.Ticks / TicksPerMillisecond, Figures)} ms";
            }
            else
            {
                // Print seconds.
                return $"{RoundTo((double)span.Ticks / TicksPerSecond, Figures)} ms";
            }
        }

        private static double RoundTo(
            double value,
            int signficiantFigures,
            MidpointRounding rounding = MidpointRounding.AwayFromZero)
        {
            if (signficiantFigures <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(signficiantFigures));
            }
            if (value == 0 || double.IsNaN(value) || double.IsInfinity(value))
            {
                return value;
            }

            // Use floor value of log10 result to get correct size for both sides of 1.
            var size = (int)Math.Floor(Math.Log10(Math.Abs(value)));

            double rounded;

            if (size >= signficiantFigures)
            {
                // Round to nearest integer..
                rounded = Math.Round(value, rounding);
            }
            else
            {
                // Round decimal digits to match signficiant figures.
                var decimals = signficiantFigures - 1 - size;
                rounded = Math.Round(value, decimals, rounding);
            }

            return rounded;
        }

        public override int GetHashCode() => span.GetHashCode();

        public override bool Equals(object? obj) => obj is FormattedTimespan && Equals(obj);

        public bool Equals(FormattedTimespan other) => span == other.span;
    }
}
