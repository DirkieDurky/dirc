using System.Diagnostics;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No file argument provided.");
            return;
        }

        string path = args[0];

        if (File.Exists(path))
        {
            string resultDir = Path.Combine(
                Path.GetDirectoryName(path) ?? "",
                Path.GetFileNameWithoutExtension(path) + ".diric"
            );

            HandleFile(path, resultDir);
        }
        else if (Directory.Exists(path))
        {
            foreach (string file in Directory.EnumerateFiles(path).Order())
            {
                if (!file.EndsWith(".dirc")) continue;

                var dir = Directory.CreateDirectory(Path.Combine(path, "builds"));

                string resultDir = Path.Combine(
                    dir.ToString(),
                    Path.GetFileNameWithoutExtension(file) + ".diric"
                );

                Console.WriteLine($"Compiling {file}...");
                try
                {
                    HandleFile(file, resultDir);
                    Console.WriteLine($"Compiled {file} with no errors.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error occurred while compiling {file}: {e}\n\n");
                }
            }
        }
        else
        {
            Console.WriteLine($"No file or directory found at {path}");
        }
    }

    private static void HandleFile(string path, string resultDir)
    {
        string text = File.ReadAllText(path);

        Compiler compiler = new();
        string[] newLines = compiler.Compile(text);

        File.WriteAllLines(resultDir, newLines);
    }
}
