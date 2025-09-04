using System.Text.Json;
using Dirc.Compiling;
using Dirc.Compiling.Parsing;
using Dirc.Linking;

namespace Dirc;

public class Driver
{
    public void Run(Options options)
    {
        List<string> files = [];

        foreach (string file in options.InputFiles)
        {
            if (File.Exists(file))
            {
                files.Add(Path.GetFullPath(file));
            }
            else
            {
                Console.Error.WriteLine($"Warning: File or pattern not found: {file}");
            }
        }

        if (files.Count == 0)
        {
            Console.Error.WriteLine("Couldn't find any given input files");
            return;
        }

        string rootFile = files.First();

        CompilationUnit compilationUnit = new CompilationUnit(files);

        Dictionary<string, FrontEndResult> frontEndResults = [];
        Dictionary<string, CompilerResult> backEndResults = [];

        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            BuildContext buildContext = new(file, compilationUnit, i == 0);
            frontEndResults.Add(file, new Compiler().RunFrontEnd(File.ReadAllText(file), options, buildContext));
            Console.WriteLine($"Successfully compiled {file}");
        }

        SymbolTable finalSymbolTable = new();
        foreach (FrontEndResult result in frontEndResults.Values)
        {
            finalSymbolTable.Combine(result.SymbolTable);
        }

        foreach ((string file, FrontEndResult result) in frontEndResults)
        {
            BuildContext buildContext = new(file, compilationUnit, file == rootFile);
            backEndResults.Add(file, new Compiler().RunBackEnd(result.AstNodes, finalSymbolTable, options, buildContext));
        }

        Console.WriteLine("Successfully compiled all files. Writing...");
        if (options.CompileOnly)
        {
            if (options.LibName != null)
            {
                Directory.CreateDirectory(options.LibName);
                string fileName = $"{options.LibName}.meta";
                string resultPath = Path.Combine(options.LibName, fileName);
                File.WriteAllText(resultPath, JsonSerializer.Serialize(finalSymbolTable));
                Console.WriteLine($"Wrote file '{fileName}' at '{resultPath}'");
            }

            foreach ((string file, CompilerResult result) in backEndResults)
            {
                string resultPath = Path.Combine(
                    options.LibName ?? Path.GetDirectoryName(file) ?? "",
                    Path.GetFileNameWithoutExtension(file) + '.' + BuildEnvironment.ObjectFileExtension
                );

                File.WriteAllText(resultPath, result.Code);
                Console.WriteLine($"Wrote file '{file}' at '{resultPath}'");
            }

            if (options.LibName != null)
            {
                Console.WriteLine($"Created library at '{options.LibName}/'");
            }
        }
        else
        {
            CompilerResult rootFileResult = backEndResults[rootFile];
            List<CompilerResult> otherCompilationUnitResults = backEndResults.Where(x => x.Key != rootFile).Select(x => x.Value).ToList();
            List<string> otherCompilationUnitCode = otherCompilationUnitResults.Select(x => x.Code).ToList();
            string[] imports = backEndResults.SelectMany(x => x.Value.Imports).ToArray();

            string linkerResult = new Linker().Link(rootFileResult.Code, otherCompilationUnitCode, imports);

            new FileInfo(options.OutPath).Directory!.Create();
            File.WriteAllText(options.OutPath, linkerResult);
            Console.WriteLine($"Executable file at '{options.OutPath}'");
        }
    }
}
