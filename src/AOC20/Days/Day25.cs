using System;

namespace AOC20.Day25
{
    public class Day25 : ISolvable
    {
        private const int Card = 2069194;
        private const int Door = 16426071;

        private const int Subject = 7;
        private const int Modulus = 20201227;

        public uint Day => 25;

        public string Title => "Combo Breaker";

        public object SolvePart1()
        {
            var cardLoopSize = FindLoopSize(Subject, Card);
            var doorLoopSize = FindLoopSize(Subject, Door);

            var cardEncryptionKey = Transform(Door, cardLoopSize);
            var doorEncryptionKey = Transform(Card, doorLoopSize);

            if (cardEncryptionKey != doorEncryptionKey)
            {
                throw new Exception($"card {cardEncryptionKey} != door {doorEncryptionKey}");
            }
            return cardEncryptionKey;
        }

        public object SolvePart2() => throw new NotImplementedException();

        private long Transform(long subject, int loopSize)
        {
            var value = 1L;
            for (var i = 0; i < loopSize; i++)
            {
                value *= subject;
                value %= Modulus;
            }
            return value;
        }

        private int FindLoopSize(long subject, long expected)
        {
            var value = 1L;
            var size = 0;
            while (value != expected)
            {
                value *= subject;
                value %= Modulus;
                size++;
            }
            return size;
        }
    }
}
