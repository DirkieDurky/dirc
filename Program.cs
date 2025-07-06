using System;
using System.IO;

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

        string[] lines = File.ReadAllLines(filePath);

        string[] newLines = Compiler.Compile(lines);

        // Create new file path with ".dirisc" extension
        string diriscPath = Path.Combine(
            Path.GetDirectoryName(filePath) ?? "",
            Path.GetFileNameWithoutExtension(filePath) + ".dirisc"
        );

        File.WriteAllLines(diriscPath, newLines);
    }
}
