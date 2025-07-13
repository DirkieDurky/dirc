namespace Dirc;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("No file argument provided.");
            return;
        }

        List<string> flags = new();
        foreach (string arg in args)
        {
            if (arg.StartsWith("--"))
            {
                flags.Add(arg.Substring("--".Length));
            }
        }

        CompilerOptions compilerOptions = new(flags);

        string path = args[0];

        if (File.Exists(path))
        {
            string resultDir = Path.Combine(
                Path.GetDirectoryName(path) ?? "",
                Path.GetFileNameWithoutExtension(path) + ".diric"
            );

            HandleFile(path, resultDir, compilerOptions);
        }
        else if (Directory.Exists(path))
        {
            foreach (string file in Directory.EnumerateFiles(path).Order())
            {
                if (!file.EndsWith(".dirc")) continue;

                DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(path, "builds"));

                string resultDir = Path.Combine(
                    dir.ToString(),
                    Path.GetFileNameWithoutExtension(file) + ".diric"
                );

                Console.WriteLine($"Compiling {file}...");
                try
                {
                    HandleFile(file, resultDir, compilerOptions);
                    Console.WriteLine($"Compiled {file} with no errors.");
                    Console.WriteLine();
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

    private static void HandleFile(string path, string resultDir, CompilerOptions compilerOptions)
    {
        string text = File.ReadAllText(path);

        Compiler compiler = new();
        string[] newLines = compiler.Compile(text, compilerOptions);

        File.WriteAllLines(resultDir, newLines);
    }
}
