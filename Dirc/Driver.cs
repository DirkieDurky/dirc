using System.Text.Json;
using Dirc.Compiling;
using Dirc.Compiling.CodeGen;
using Dirc.Compiling.Parsing;
using Dirc.Linking;

namespace Dirc;

public class Driver
{
    public void Run(Options options)
    {
        BuildEnvironment buildEnvironment = new(options);
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

        string rootFile;
        if (options.EntryPoint != null)
        {
            rootFile = files.First(f => Helper.FilesMatch(f, options.EntryPoint));
        }
        else
        {
            rootFile = files.First();
        }

        CompilationUnit compilationUnit = new CompilationUnit(files);

        Dictionary<string, FrontEndResult> frontEndResults = [];
        Dictionary<string, CompilerResult> backEndResults = [];

        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];
            BuildContext buildContext = new(file, compilationUnit, Helper.FilesMatch(file, rootFile));
            frontEndResults.Add(file, new Compiler().RunFrontEnd(File.ReadAllText(file), options, buildContext, buildEnvironment));
        }

        SymbolTable finalSymbolTable = new();
        foreach (FrontEndResult result in frontEndResults.Values)
        {
            finalSymbolTable.Combine(result.SymbolTable);
        }

        foreach ((string file, FrontEndResult result) in frontEndResults)
        {
            BuildContext buildContext = new(file, compilationUnit, Helper.FilesMatch(file, rootFile));
            backEndResults.Add(file, new Compiler().RunBackEnd(result.AstNodes, finalSymbolTable, options, buildContext, buildEnvironment));
            Console.WriteLine($"Successfully compiled '{Helper.GetRelativePath(file)}'");
        }

        if (options.OutPath == null)
        {
            if (options.ExportingAsLibrary)
            {
                options.OutPath = options.LibName!;
            }
            else if (options.CompileOnly)
            {
                options.OutPath = "out";
            }
            else
            {
                options.OutPath = "a.out";
            }
        }

        if (options.CompileOnly && !Directory.Exists(options.OutPath))
        {
            Directory.CreateDirectory(options.OutPath);
        }

        Console.WriteLine("Successfully compiled all files. Writing...");
        if (options.CompileOnly)
        {
            if (options.ExportingAsLibrary)
            {
                string fileName = $"{options.LibName}.meta";
                string resultPath = Path.Combine(options.OutPath, fileName);
                File.WriteAllText(resultPath, JsonSerializer.Serialize(finalSymbolTable));
                Console.WriteLine($"Wrote meta file at '{Helper.GetRelativePath(resultPath)}'");
            }

            foreach ((string file, CompilerResult result) in backEndResults)
            {
                string sourceFilePath = Helper.GetRelativePath(Path.GetDirectoryName(file)!);

                string resultPath = Path.Combine(
                    options.OutPath,
                    Path.GetFileNameWithoutExtension(file) + '.' + BuildEnvironment.ObjectFileExtension
                );

                Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);
                File.WriteAllText(resultPath, result.Code);
                Console.WriteLine($"Wrote results of source file '{Helper.GetRelativePath(file)}' at '{Helper.GetRelativePath(resultPath)}'");
            }

            if (options.LibName != null)
            {
                Console.WriteLine($"Final library at '{Helper.GetRelativePath(options.OutPath)}{Path.DirectorySeparatorChar}'");
            }
        }
        else
        {
            CompilerResult rootFileResult = backEndResults[rootFile];
            List<CompilerResult> otherCompilationUnitResults = backEndResults.Where(x => x.Key != rootFile).Select(x => x.Value).ToList();
            List<string> otherCompilationUnitCode = otherCompilationUnitResults.Select(x => x.Code).ToList();
            string[] imports = backEndResults.SelectMany(x => x.Value.Imports).ToArray();

            string linkerResult = new Linker().Link(rootFileResult.Code, otherCompilationUnitCode, imports, buildEnvironment);

            new FileInfo(options.OutPath).Directory!.Create();
            File.WriteAllText(options.OutPath, linkerResult);
            Console.WriteLine($"Executable file at '{Helper.GetRelativePath(options.OutPath)}'");
        }
    }
}
