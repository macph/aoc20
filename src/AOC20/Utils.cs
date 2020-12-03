using System;
using System.Collections.Generic;
using System.IO;

namespace AOC20
{
    public static class Utils
    {
        public static Stream OpenResource(string resource) =>
            typeof(Utils).Assembly.GetManifestResourceStream(resource)
                ?? throw new Exception($"Resource '{resource}' not found");

        public static IEnumerable<string> ReadLines(StreamReader reader, bool skipEmpty = true)
        {
            while (true)
            {
                var line = reader.ReadLine();

                if (line is null) break;
                if (skipEmpty && string.IsNullOrWhiteSpace(line)) continue;

                yield return line;
            }
        }

        public static IEnumerable<string> ReadLines(Stream stream, bool skipEmpty = true)
        {
            using var reader = new StreamReader(stream);
            foreach (string line in ReadLines(reader, skipEmpty))
            {
                yield return line;
            }
        }
    }
}
