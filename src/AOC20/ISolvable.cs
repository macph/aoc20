namespace AOC20
{
    public interface ISolvable
    {
        uint Day { get; }

        object SolvePart1();

        object SolvePart2();
    }
}
