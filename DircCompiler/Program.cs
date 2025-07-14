namespace Dirc;

class Program
{
    static void Main(string[] args)
    {
        List<string> flags = new();
        List<string> argsList = new();
        foreach (string arg in args)
        {
            if (arg.StartsWith("-"))
            {
                flags.Add(arg);
            }
            else
            {
                argsList.Add(arg);
            }
        }

        CompilerOptions compilerOptions = new(flags);

        if (argsList.Count == 0)
        {
            Console.WriteLine("No file argument provided.");
            return;
        }

        string sourcePath = argsList[0];

        if (File.Exists(sourcePath))
        {
            string resultPath = Path.Combine(
                Path.GetDirectoryName(sourcePath) ?? "",
                Path.GetFileNameWithoutExtension(sourcePath) + ".diric"
            );

            CompilerContext compilerContext = new(sourcePath);
            HandleFile(sourcePath, resultPath, compilerOptions, compilerContext);
        }
        else if (Directory.Exists(sourcePath))
        {
            foreach (string sourceFile in Directory.EnumerateFiles(sourcePath).Order())
            {
                if (!sourceFile.EndsWith(".dirc")) continue;

                DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(sourcePath, "builds"));

                string resultDir = Path.Combine(
                    dir.ToString(),
                    Path.GetFileNameWithoutExtension(sourceFile) + ".diric"
                );

                Console.WriteLine($"Compiling {sourceFile}...");
                try
                {
                    CompilerContext compilerContext = new(sourceFile);
                    HandleFile(sourceFile, resultDir, compilerOptions, compilerContext);
                    Console.WriteLine($"Compiled {sourceFile} with no errors.");
                    Console.WriteLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error occurred while compiling {sourceFile}: {e}\n\n");
                }
            }
        }
        else
        {
            Console.WriteLine($"No file or directory found at {sourcePath}");
        }
    }

    private static void HandleFile(string path, string resultDir, CompilerOptions compilerOptions, CompilerContext compilerContext)
    {
        string text = File.ReadAllText(path);

        Compiler compiler = new();
        string[] newLines = compiler.Compile(text, compilerOptions, compilerContext);

        File.WriteAllLines(resultDir, newLines);
    }
}
