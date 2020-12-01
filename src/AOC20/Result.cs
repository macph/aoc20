using System;

namespace AOC20
{
    public class Solution
    {
        public uint Day { get; }
        public TimeSpan Elapsed1 { get; }
        public object Result1 { get; }
        public TimeSpan Elapsed2 { get; }
        public object Result2 { get; }

        public Solution(
            uint day,
            TimeSpan elapsed1,
            object result1,
            TimeSpan elapsed2,
            object result2)
        {
            Day = day;
            Elapsed1 = elapsed1;
            Result1 = result1;
            Elapsed2 = elapsed2;
            Result2 = result2;
        }
    }
}
