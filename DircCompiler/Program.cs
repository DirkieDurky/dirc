namespace DircCompiler;

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
            Console.WriteLine($"Successfully compiled {sourcePath}. Compiled file at {resultPath}");
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
                    Console.WriteLine($"Successfully compiled {sourceFile}. Compiled file at {resultDir}");
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
        Compiler compiler = new();

        //Compile any uncompiled .dirc files found in lib.
        string libPath = Path.Combine(AppContext.BaseDirectory, "lib");
        List<string> uncompiledFiles = Directory.EnumerateFiles(libPath, "*.dirc", SearchOption.AllDirectories).ToList();
        bool logged = false;
        foreach (string file in uncompiledFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            string containingFolder = Path.GetDirectoryName(file)!;
            string newFilePath = Path.Combine(containingFolder, fileName + ".diric");

            if (!File.Exists(newFilePath))
            {
                if (!logged)
                {
                    Console.WriteLine("Found at least 1 uncompiled .dirc file in lib folder. Compiling those first...");
                    logged = true;
                }

                string libFileText = File.ReadAllText(file);
                CompilerContext libCompilerContext = new CompilerContext(file);
                string[] libFileNewLines = compiler.Compile(libFileText, compilerOptions, libCompilerContext);
                File.WriteAllLines(newFilePath, libFileNewLines);
            }
        }

        string text = File.ReadAllText(path);
        string[] newLines = compiler.Compile(text, compilerOptions, compilerContext);

        File.WriteAllLines(resultDir, newLines);
    }
}
