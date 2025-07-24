public static class StringExtensions
{
    public static string TrimIndents(this string input)
    {
        var lines = input.Split('\n')
                         .Select(line => line.TrimEnd())
                         .SkipWhile(string.IsNullOrWhiteSpace)
                         .ToArray();

        int minIndent = lines
            .Where(line => line.Trim().Length > 0)
            .Select(line => line.TakeWhile(char.IsWhiteSpace).Count())
            .DefaultIfEmpty(0)
            .Min();

        return string.Join('\n', lines.Select(line => line.Length >= minIndent ? line[minIndent..] : line));
    }
}
