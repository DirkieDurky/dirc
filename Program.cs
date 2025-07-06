class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No file argument provided.");
            return;
        }

        string filePath = args[0];
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"File not found: {filePath}");
            return;
        }

        string text = File.ReadAllText(filePath);

        Compiler compiler = new(text);
        string[] newLines = compiler.Compile();

        // Create new file path with ".dirisc" extension
        string diriscPath = Path.Combine(
            Path.GetDirectoryName(filePath) ?? "",
            Path.GetFileNameWithoutExtension(filePath) + ".diric"
        );

        File.WriteAllLines(diriscPath, newLines);
    }
}
