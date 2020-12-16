namespace AOC20
{
    public interface ISolvable
    {
        uint Day { get; }

        string Title { get; }

        object SolvePart1();

        object SolvePart2();
    }
}
